using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//SCSM references
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Workflow.Common;
using Microsoft.EnterpriseManagement.ConnectorFramework;
//workflow references
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using System.Workflow.Runtime;
//project adapter reference
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.InteropHelper;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.Licensing;
//other references
using System.Xml;
using System.Xml.XPath;
using System.Net;
using System.Diagnostics;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.Classes.Licensing;
//using System.Security;
//project references
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;


// This workflow process is in .NET 4.5 so we can use the native Project Server assemblies.  Service Manager seems to work with a .NET 4.5 compiled workflow assembly so
//  I'll run with it for now.
namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows
{
    public partial class ProcessProjectCIs : WorkflowActivityBase
    {
        public static DependencyProperty ConnectorIdProperty = DependencyProperty.Register("ConnectorId", typeof(Guid), typeof(ProcessProjectCIs));
        public Guid ConnectorId
        {
            get 
            { 
                return (Guid)this.GetValue(ConnectorIdProperty); 
            }
            set
            {
                this.SetValue(ConnectorIdProperty, value);
            }
        }

        EnterpriseManagementGroup emg;
        EnterpriseManagementObject emoStatus;
        EnterpriseManagementObject emoSettings;
        EnterpriseManagementObject emoConnector;
        IncrementalDiscoveryData iddObjects;
        IList<ManagementPack> managementPacks;
        IList<ManagementPackClass> mpcClasses;
        IList<ManagementPackRelationship> mprRelationships;
        IList<ManagementPackEnumeration> mpeSyncStatusEnums;
        bool hasImportFailure = false;
        DateTime startTime;

        //tracking fields
        int totalToProcess = 0;
        int totalProcessed = 0;

        static string xmlOwnerCriteria = @"<Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">   
                            <Expression>
                                <SimpleExpression>
                                    <ValueExpressionLeft>
                                        <Property>$Context/Property[Type='Microsoft.AD.UserBase']/SID$</Property>
                                    </ValueExpressionLeft>
                                    <Operator>Equal</Operator>
                                    <ValueExpressionRight>
                                    <Value>{0}</Value>
                                    </ValueExpressionRight>
                                </SimpleExpression>
                            </Expression>
                        </Criteria>";

        static string strExceptionMessage = "Message: {0}\r\nInner Exception: {1}\r\nStack Trace: {2}\r\nSource: {3}";
        static string strEventLogTitle = "Project Connector";

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            try
            {

                //System.Threading.Thread.Sleep(10000); //makes debugging easier.
                if(ConnectorId == null)
                    throw new ArgumentNullException("ConnectorId is null.");

                startTime = DateTime.UtcNow; //set the start time.

                //setup event log access
                if(!EventLog.SourceExists(strEventLogTitle)) //check if WF account is non-admin
                    EventLog.CreateEventSource(strEventLogTitle, "Operations Manager");
                
                #region Setup connection to EMG and get objects.
                emg = new EnterpriseManagementGroup("localhost");

                setEnterpriseManagementObjects();

                //get the last run date time so we can get projects with a last modified date greater than the last run. Check for UTC on project...
                DateTime lastRun = emoStatus[null, "LastRunFinishTime"].Value == null ? DateTime.MinValue : (DateTime)emoStatus[null, "LastRunFinishTime"].Value;

                //check to see if retention is enabled.  If so, and this is the first run, change the lastRun date.
                var retentionEnabled = emoSettings[null, "IsRetentionEnabled"].Value == null ? false : (bool)emoSettings[null, "IsRetentionEnabled"].Value;
                if (retentionEnabled && DateTime.Equals(lastRun, DateTime.MinValue))
                    lastRun = DateTime.Now.AddDays(-(Convert.ToDouble(emoSettings[null, "RetentionDays"].Value)));

                //set the start time on the status object.
                emoStatus[null, "LastRunStartTime"].Value = startTime;
                emoStatus[null, "LastRunFinishTime"].Value = null;
                emoStatus[null, "Status"].Value = mpeSyncStatusEnums.First(mpe => mpe.Name == "Microsoft.SystemCenter.LinkingFramework.SyncStatusEnum.Running");
                emoStatus[null, "MinValue"].Value = 0;
                emoStatus[null, "MaxValue"].Value = 100;
                #endregion

                #region Get connector credentails
                NetworkCredential creds = NetworkCredentialsHelper.GetProjectCredentials(emoConnector[null, "RunAsAccount"].Value.ToString(), emoConnector[null, "Id"].ToString(), emg.Name);
                #endregion

                EventLog.WriteEntry(strEventLogTitle, string.Format("Starting Project CI sync from Project Server...\r\n\r\nProject Server: {0}\r\nUser Name: {1}", emoConnector[null, "ProjectServerURL"].Value.ToString(), creds.Domain + "\\" + creds.UserName));

                #region license check
                var licenseExpiration = checkLicense(); //method will throw an exception if no license exists.
                EventLog.WriteEntry(strEventLogTitle, string.Format("License Expiration: {0}", licenseExpiration));
                #endregion

                #region Connect to Project Server and read data
                ProjectConnector projectConnector = new ProjectConnector(emoConnector[null, "ProjectServerURL"].Value.ToString(), creds);
                projectConnector.Connect();

                #endregion

                #region Read data from Project Server

                if(lastRun == DateTime.MinValue)
                    EventLog.WriteEntry(strEventLogTitle, "Querying Project Server for a full list of projects...");
                else
                    EventLog.WriteEntry(strEventLogTitle, string.Format("Querying Project Server for Projects created or modified since: {0} UTC...", lastRun));

                projectConnector.LoadProjects(lastRun);

                #endregion

                #region Write data to the Service Manager DB.

                totalToProcess = projectConnector.Projects.Count();
                if (totalToProcess > 0)
                {
                    EventLog.WriteEntry(strEventLogTitle, string.Format("Processing {0} projects returned from Project Server...", totalToProcess), EventLogEntryType.Information, 0);

                    iddObjects = new IncrementalDiscoveryData();
                    List<Exception> exceptionsList = new List<Exception>();

                    foreach (PublishedProject project in projectConnector.Projects)
                    {
                        try
                        {
                            projectConnector.LoadProjectTaskData(project);
                            projectConnector.LoadProjectMetaData(project);

                            iddObjects.Add(buildProjectCI(project, mpcClasses.First(c => c.Name == "Cireson.ProjectAutomation.Project")));
                            
                        }
                        catch(EnterpriseManagementException ex)
                        {
                            exceptionsList.Add(new ProjectImportException(string.Format("An error occured while importing the following project:\r\n\r\nProject Name: {0}\r\n{1}\r\n", project.Name, ex)));
                            hasImportFailure = true;
                        }
                        //catch SP exceptions
                        finally
                        {
                            totalProcessed++;
                            updateWorkflowStatus(false, ((totalProcessed * 100) / totalToProcess));
                        }

                    }//end of foreach.

                #endregion

                    //commit the changes to the database
                    iddObjects.Overwrite(emg);

                    if (exceptionsList.Count > 0)
                        throw new AggregateException(string.Format("{0} projects failed to sync. ", (totalToProcess - exceptionsList.Count).ToString()), exceptionsList);  
                    else
                        EventLog.WriteEntry(strEventLogTitle, string.Format("{0} projects successfully committed.  Elapsed Time: {1} seconds.", totalProcessed, getRunTime(startTime).Seconds), EventLogEntryType.Information, 0);
                }
                else
                    EventLog.WriteEntry(strEventLogTitle, string.Format("No updated projects exist.  Project Connector workflow is exiting.  Elapsed Time: {0} seconds.", getRunTime(startTime).Seconds));

              

            } 
            catch(AggregateException ex)
            {
                TrackData(ex.Message);

                StringBuilder exceptionStrings = new StringBuilder();
                foreach(var innerEx in ex.InnerExceptions)
                    exceptionStrings.AppendLine(string.Format(strExceptionMessage, innerEx.Message, innerEx.InnerException, innerEx.StackTrace, innerEx.Source));

                EventLog.WriteEntry(strEventLogTitle, string.Format(ex.Message + "Elapsed Time: {0} seconds. The following projects failed to sync: \r\n\r\n{1}", getRunTime(startTime).Seconds, exceptionStrings.ToString()), EventLogEntryType.Error, 1);
                hasImportFailure = true;
            }
            catch(EnterpriseManagementException ex)
            {
                TrackData(ex.Message);
                EventLog.WriteEntry(strEventLogTitle, string.Format(strExceptionMessage, ex.Message, ex.InnerException, ex.StackTrace, ex.Source), EventLogEntryType.Error, 1);
                hasImportFailure = true;
                throw;
            }
            catch(Exception ex)
            {
                TrackData(ex.Message);
                EventLog.WriteEntry(strEventLogTitle, string.Format(strExceptionMessage, ex.Message, ex.InnerException, ex.StackTrace, ex.Source), EventLogEntryType.Error, 1);
                hasImportFailure = true;
                throw;
            }
            finally 
            {
                updateWorkflowStatus(true, 100);

                if(emg != null)
                    emg.Dispose();

            }
            return ActivityExecutionStatus.Closed;
        }

        TimeSpan getRunTime(DateTime runTime)
        {
            return DateTime.UtcNow.Subtract(runTime);
        }

        void setEnterpriseManagementObjects()
        {

            managementPacks = emg.ManagementPacks.GetManagementPacks(
                new List<Guid>()
                    {
                        new Guid("545131f0-58de-1914-3a82-4fcac9100a33") //Management pack containing AD User or Group. Windows Core Library (Microsoft.Windows.Library) (545131f0-58de-1914-3a82-4fcac9100a33)
                    });

            mpcClasses = emg.EntityTypes.GetClasses(
                new List<Guid>()
                {
                    //new Guid("d581d2d6-b6cd-b558-7ac7-db233a7c82ec"), //Project Server Connector (Cireson.ProjectAutomation.Library) (Microsoft.SystemCenter.Connector.ProjectServer) (d581d2d6-b6cd-b558-7ac7-db233a7c82ec)
                    new Guid("5a49b80c-4c34-d189-ca94-a591580f1995"), // (Cireson.ProjectAutomation.Library) (Microsoft.SystemCenter.Connector.ProjectServer.Settings) (5a49b80c-4c34-d189-ca94-a591580f1995)
                    new Guid("3567434d-015f-8dcc-f188-0a407f3a2168"), ////Active Directory User or Group (Microsoft.Windows.Library) (Microsoft.AD.UserBase) (3567434d-015f-8dcc-f188-0a407f3a2168)
                    //new Guid("2d4afd51-d2ff-92c6-266f-2b6060000dae"),  //Synchronization Status (ServiceManager.LinkingFramework.Library) (Microsoft.SystemCenter.LinkingFramework.SyncStatus) (2d4afd51-d2ff-92c6-266f-2b6060000dae)
                    new Guid("dabf07ea-6047-b6d7-f8d3-e88ac1d260e9") //Cireson Project (Cireson.ProjectAutomation.Library) (Cireson.ProjectAutomation.Project) (dabf07ea-6047-b6d7-f8d3-e88ac1d260e9)
                });

            mprRelationships = emg.EntityTypes.GetRelationshipClasses(
                new List<Guid>()
                    {
                        new Guid("1548950d-6cea-d9c1-11ec-53701fbcbbec"), // (ServiceManager.LinkingFramework.Library) (Microsoft.SystemCenter.LinkingFramework.DataSourceHostSyncStatus) (1548950d-6cea-d9c1-11ec-53701fbcbbec)
                        new Guid("da061582-3f6c-d7b7-d17d-0a91b8a51ace"),  // (Cireson.ProjectAutomation.Library) (System.ProjectConfigItemRelatesToProjectConnector) (da061582-3f6c-d7b7-d17d-0a91b8a51ace)
                        new Guid("721c14fc-8a89-c931-d8e8-65270280c3f6")  //Project Has Owner (Cireson.ProjectAutomation.Library) (Cireson.ProjectAutomation.ProjectHasOwner) (721c14fc-8a89-c931-d8e8-65270280c3f6)
                    });

            //Synchronization Status (ServiceManager.LinkingFramework.Library) (Microsoft.SystemCenter.LinkingFramework.SyncStatusEnum) (8d219d62-45f3-1f92-8b7e-eae65b24c42a)
            mpeSyncStatusEnums = emg.EntityTypes.GetChildEnumerations(new Guid("8d219d62-45f3-1f92-8b7e-eae65b24c42a"), TraversalDepth.OneLevel);

            emoSettings = emg.EntityObjects.GetObject<EnterpriseManagementObject>(
                mpcClasses.First(mpc => mpc.Name == "Microsoft.SystemCenter.Connector.ProjectServer.Settings").Id, ObjectQueryOptions.Default); //get the settings class instance.

            emoConnector = emg.EntityObjects.GetObject<EnterpriseManagementObject>(ConnectorId, ObjectQueryOptions.Default);

            //get the status object for the connector

            foreach (EnterpriseManagementRelationshipObject<EnterpriseManagementObject> obj in emg.EntityObjects.GetRelationshipObjectsWhereSource<EnterpriseManagementObject>(
                emoConnector.Id, TraversalDepth.OneLevel, ObjectQueryOptions.Default))
                if (obj.RelationshipId == mprRelationships.First(r => r.Name == "Microsoft.SystemCenter.LinkingFramework.DataSourceHostSyncStatus").Id)
                {
                    emoStatus = obj.TargetObject;
                    break;
                }
        }

        EnterpriseManagementObject buildProjectCI(PublishedProject project, ManagementPackClass mpcProject)
        {
            //get the project class from service manager
            CreatableEnterpriseManagementObject cemoProject = new CreatableEnterpriseManagementObject(emg, mpcProject);

            #region set Project properties...
            cemoProject[mpcProject, "GUID"].Value = project.Id;
            cemoProject[mpcProject, "ProjectCreatedDate"].Value = project.CreatedDate as DateTime?;
            cemoProject[mpcProject, "ProjectLastModified"].Value = project.LastPublishedDate as DateTime?;
            cemoProject[mpcProject, "Title"].Value = project.Name as string;
            cemoProject[mpcProject, "DisplayName"].Value = project.Name as string;

            //the next project properties need the data loaded.

            cemoProject[mpcProject, "NextMilestone"].Value = project.Tasks.Where(t => t.IsMilestone == true).FirstOrDefault() == null ? string.Empty : project.Tasks.Where(t => t.IsMilestone == true).FirstOrDefault().Name;


            cemoProject[mpcProject, "ProjectCalendar"].Value = project.Calendar.Name;
            cemoProject[mpcProject, "ProjectStartDate"].Value = project.StartDate as DateTime?;
            cemoProject[mpcProject, "ProjectEndDate"].Value = project.FinishDate as DateTime?;
            cemoProject[mpcProject, "ProjectWebUrl"].Value = project.ProjectSiteUrl as string;
            cemoProject[mpcProject, "Description"].Value = project.Description as string;
            cemoProject[mpcProject, "PercentComplete"].Value = project.PercentComplete + "%";

            #endregion

            #region set Project relationships...
            setProjectHasOwnerRelationship(project.Owner.UserId.NameId, ref cemoProject);
            //create a relationship between the connector and project.
            setProjectConnectorRelationship(ref cemoProject);

            #endregion

            #region set Project enums...
            setProjectEnumValues(ref cemoProject, mpcProject, project);

            EventLog.WriteEntry(strEventLogTitle, string.Format("Project GUID: {0}, Name: {1} successfully processed.", cemoProject.Id, cemoProject[null, "DisplayName"].Value), EventLogEntryType.Information, 0);

            return cemoProject;
            #endregion
        }

        void setProjectEnumValues(ref CreatableEnterpriseManagementObject cemoProject, ManagementPackClass mpcProject, PublishedProject project)
        {
            //set object status
            //Active (System.Library) (System.ConfigItem.ObjectStatusEnum.Active) (acdcedb7-100c-8c91-d664-4629a218bd94)
            cemoProject[mpcProject, "ObjectStatus"].Value = emg.EntityTypes.GetEnumeration(new Guid("acdcedb7-100c-8c91-d664-4629a218bd94"));

            //Project Status (Cireson.ProjectAutomation.Library) (Cireson.ProjectAutomation.ProjectStatusEnum) (3dbbdf0b-9b1e-1fbf-26f7-b09743fb5604)
            IList<ManagementPackEnumeration> projectStatusEnums = emg.EntityTypes.GetChildEnumerations(new Guid("3dbbdf0b-9b1e-1fbf-26f7-b09743fb5604"), TraversalDepth.OneLevel);

            //set project status
            //Completed (Cireson.ProjectAutomation.Library) (Cireson.ProjectAutomation.ProjectStatusEnum.Completed) (08cbcd28-a84a-67fe-ce4b-4be2c5320b2a)
            //In Progress (Cireson.ProjectAutomation.Library) (Cireson.ProjectAutomation.ProjectStatusEnum.InProgress) (0e5da073-9d56-c418-a05b-4026bfdfe8b0)
            //New (Cireson.ProjectAutomation.Library) (Cireson.ProjectAutomation.ProjectStatusEnum.New) (fec278c4-3761-1283-d4a6-6ca13320fe0e)
            if ((bool)emoSettings[null, "IsNewStatus"].Value && project.PercentComplete == 0)
                cemoProject[mpcProject, "ProjectStatus"].Value = projectStatusEnums.First(mpe => mpe.Name == "Cireson.ProjectAutomation.ProjectStatusEnum.New");
            if ((bool)emoSettings[null, "IsInProgressStatus"].Value && (project.PercentComplete != 0 && project.PercentComplete != 100))
                cemoProject[mpcProject, "ProjectStatus"].Value = projectStatusEnums.First(mpe => mpe.Name == "Cireson.ProjectAutomation.ProjectStatusEnum.InProgress");
            if ((bool)emoSettings[null, "IsCompletedStatus"].Value && project.PercentComplete == 100)
                cemoProject[mpcProject, "ProjectStatus"].Value = projectStatusEnums.First(mpe => mpe.Name == "Cireson.ProjectAutomation.ProjectStatusEnum.Completed");
        }

        void setProjectConnectorRelationship(ref CreatableEnterpriseManagementObject project)
        {
            CreatableEnterpriseManagementRelationshipObject cemroConnector =
                    new CreatableEnterpriseManagementRelationshipObject(emg, mprRelationships.First(r => r.Name == "System.ProjectConfigItemRelatesToProjectConnector"));
            cemroConnector.SetSource(emoConnector);
            cemroConnector.SetTarget(project);
            iddObjects.Add(cemroConnector);
        }

        void setProjectHasOwnerRelationship(string nameId, ref CreatableEnterpriseManagementObject cemoProject)
        {
            //get the owner of the project as an EMO.
            EnterpriseManagementObjectCriteria ecoOwnerCriteria = new EnterpriseManagementObjectCriteria(string.Format(xmlOwnerCriteria, nameId),
                mpcClasses.First(mpc => mpc.Name == "Microsoft.AD.UserBase"), managementPacks.First(mp => mp.Name == "Microsoft.Windows.Library"), emg);
            EnterpriseManagementObject emoOwner = emg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(ecoOwnerCriteria, ObjectQueryOptions.Default).FirstOrDefault();

            if (emoOwner != null)
            {
                //set up the relationship between project owner and project.
                CreatableEnterpriseManagementRelationshipObject cemroOwner =
                    new CreatableEnterpriseManagementRelationshipObject(emg, mprRelationships.First(r => r.Name == "Cireson.ProjectAutomation.ProjectHasOwner"));
                cemroOwner.SetSource(cemoProject);
                cemroOwner.SetTarget(emoOwner);
                iddObjects.Add(cemroOwner);
            }
        }

        void updateWorkflowStatus(bool isFinished, double percentComplete = 0)
        {
            //Finished Success (ServiceManager.LinkingFramework.Library) (Microsoft.SystemCenter.LinkingFramework.SyncStatusEnum.FinishedSuccess) (c277e4d1-d44a-0f6d-5f04-eda27ac426f8)
            //Finished With Error (ServiceManager.LinkingFramework.Library) (Microsoft.SystemCenter.LinkingFramework.SyncStatusEnum.FinishedWithError) (ef06aee3-cc98-264d-b265-748a36b685de)
            //Running (ServiceManager.LinkingFramework.Library) (Microsoft.SystemCenter.LinkingFramework.SyncStatusEnum.Running) (d34da32a-5ea5-40c0-8afe-06388cf97bf3)
            try
            {

                if(!isFinished)
                    emoStatus[null, "SyncPercent"].Value = percentComplete;
                else
                {
                    emoStatus[null, "LastRunFinishTime"].Value = DateTime.UtcNow;
                    if (hasImportFailure)
                        emoStatus[null, "Status"].Value = mpeSyncStatusEnums.First(mpe => mpe.Name == "Microsoft.SystemCenter.LinkingFramework.SyncStatusEnum.FinishedWithError");
                    else
                    {
                        emoStatus[null, "Status"].Value = mpeSyncStatusEnums.First(mpe => mpe.Name == "Microsoft.SystemCenter.LinkingFramework.SyncStatusEnum.FinishedSuccess");
                        emoStatus[null, "SyncPercent"].Value = percentComplete;
                    }  
                }
                emoStatus.Commit();
            }
            catch
            { }
        }

        string checkLicense()
        {
            IX1 x = new X1(emg, emoSettings.Id, "Project Connector");
            return x.CheckLicense();
        }

    }
}

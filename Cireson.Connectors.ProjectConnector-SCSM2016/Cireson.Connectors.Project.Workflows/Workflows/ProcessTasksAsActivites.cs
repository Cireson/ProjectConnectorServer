


//////////////////////////////////////////////////////////////////////////////
//This file is part of Cireson Project Connector. 
//
//Cireson Project Connector is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//Cireson Project Connector is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Cireson Project Connector.  If not, see<https://www.gnu.org/licenses/>.
/////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//SCSM references
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Workflow.Common;
using Microsoft.EnterpriseManagement.ConnectorFramework;
//workflow references
//project adapter reference
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.InteropHelper;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.Licensing;
//project references
using Microsoft.ProjectServer.Client;
//other references
using System.Net;
using System.Diagnostics;
using System.Workflow.ComponentModel;

using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.Classes.Licensing;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows
{
    public partial class ProcessTasksAsActivites : WorkflowActivityBase
    {
        // here we will recieve the the ID of the activity in the RR that is linked to the DA.  We get the current activity, its parent (the RR), we get the linked DA, and the DAs parent 
        // (which is the change request).  When we have the change request, we can get the related project and the project ID.  We can then query project server to pull in the tasks
        // as manual activities and related them to the release record.

        EnterpriseManagementGroup emg;
        ProjectConnector projectConnector;
        IncrementalDiscoveryData iddBucket;
        EnterpriseManagementObject emoProjectAutomationSettings;
        EnterpriseManagementObject emoConnector;

        IList<ManagementPackEnumeration> activityStatusEnums;
        IList<ManagementPackEnumeration> releaseStatusEnums;
        IList<ManagementPackRelationship> mprRelationships;
        IList<ManagementPackClass> mpcClasses;
        IList<ManagementPack> mpManagementPacks;

        List<Exception> exceptionsList;

        DateTime startTime;

        static string sTaskActivityPrefix;
        static string strExceptionMessage = "Message: {0} \r\nInner Exception: {1} \r\nStack Trace: {2} \r\nSource: {3}";
        static string strEventLogTitle = "Project Connector Task Activity Workflow";

        #region xml criteria
        static string xmlResourceCriteria = @"<Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">   
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
        //criteria xml for all release records in an 'Editing' or 'In-Progress' status.
        static string xmlReleaseRecordHasProjectCriteria = @"<Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">
                                <Expression>
                                  <Or>
                                   <Expression>
                                     <SimpleExpression>
                                         <ValueExpressionLeft>
                                           <Property>$Context/Property[Type='System.WorkItem.ReleaseRecord']/Status$</Property>
                                         </ValueExpressionLeft>
                                         <Operator>Equal</Operator>
                                         <ValueExpressionRight>
                                           <Value>{f71c86cf-afbd-debf-4464-52fe122b888b}</Value>
                                         </ValueExpressionRight>
                                      </SimpleExpression>
                                    </Expression>
                                    <Expression>
                                     <SimpleExpression>
                                         <ValueExpressionLeft>
                                           <Property>$Context/Property[Type='System.WorkItem.ReleaseRecord']/Status$</Property>
                                         </ValueExpressionLeft>
                                         <Operator>Equal</Operator>
                                         <ValueExpressionRight>
                                           <Value>{1840bfdc-3589-88a5-cea9-67536fd95a3b}</Value>
                                         </ValueExpressionRight>
                                     </SimpleExpression>
                                   </Expression>
                                  </Or>
                                </Expression>
                               </Criteria>";

        //critera xml for all change requests that are in progress 
        static string xmlChangeRequestHasProjectCriteria = @"<Criteria xmlns=""http://Microsoft.EnterpriseManagement.Core.Criteria/"">
                                <Expression>
                                    <SimpleExpression>
                                      <ValueExpressionLeft>
                                        <Property>$Context/Property[Type='System.WorkItem.ChangeRequest']/Status$</Property>
                                      </ValueExpressionLeft>
                                      <Operator>Equal</Operator>
                                      <ValueExpressionRight>
                                        <Value>{6d6c64dd-07ac-aaf5-f812-6a7cceb5154d}</Value>
                                      </ValueExpressionRight>
                                    </SimpleExpression>
                                  </Expression>
                               </Criteria>";
        #endregion

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            try
            {
                //setup event log access
                if (!EventLog.SourceExists(strEventLogTitle))
                    EventLog.CreateEventSource(strEventLogTitle, "Operations Manager");

                startTime = DateTime.UtcNow; //set the start time

                EventLog.WriteEntry(strEventLogTitle, string.Format("Starting {0}.", strEventLogTitle));

                exceptionsList = new List<Exception>();

                #region setup connection to EMG and get objects
                emg = new EnterpriseManagementGroup("localhost");

                //sleep so I can debug...
                //System.Threading.Thread.Sleep(10000);

                setupEnterpriseManagementObjects();

                #endregion  

                var licenseExpiration = checkLicense();
                EventLog.WriteEntry(strEventLogTitle, string.Format("License Expiration: {0}", licenseExpiration));

                #region Connect to Project Server.
                //get connector credentails
                NetworkCredential creds = NetworkCredentialsHelper.GetProjectCredentials(emoConnector[null, "RunAsAccount"].Value.ToString(), emoConnector[null, "Id"].ToString(), emg.Name);
                projectConnector = new ProjectConnector(emoConnector[null, "ProjectServerURL"].Value.ToString(), creds);
                projectConnector.Connect();
                #endregion


                //setup the bucket
                iddBucket = new IncrementalDiscoveryData();

                if ((bool)emoProjectAutomationSettings[null, "IsChangeProcessingEnabled"].Value)
                    processChangeRequests();
                else
                    processReleaseRecords();

                iddBucket.Overwrite(emg); //commit changes to DB and dispose connection.

                if(exceptionsList.Count > 0)
                    throw new AggregateException("Errors occured while processing Project Tasks.", exceptionsList);  

                EventLog.WriteEntry(strEventLogTitle, string.Format("Ending {0}.  Elapsed Time: {1} seconds", strEventLogTitle, getRunTime(startTime).Seconds));

                
            }
            catch(AggregateException ex)
            {
                TrackData(ex.Message);

                StringBuilder exceptionStrings = new StringBuilder();
                foreach (var innerEx in ex.InnerExceptions)
                    exceptionStrings.AppendLine(string.Format(strExceptionMessage, innerEx.Message, innerEx.InnerException, innerEx.StackTrace, innerEx.Source));

                EventLog.WriteEntry(strEventLogTitle, string.Format("{0}\r\n\r\nElapsed Time: {1} seconds.\r\n\r\nExceptions List: {2}", ex.Message, getRunTime(startTime).Seconds, exceptionsList.ToString()));
            }
            catch (EnterpriseManagementException ex)
            {
                TrackData(ex.Message);
                EventLog.WriteEntry(strEventLogTitle, string.Format(strExceptionMessage, ex.Message, ex.InnerException, ex.StackTrace, ex.Source), EventLogEntryType.Error, 1);
                throw;
            }
            catch (Exception ex)
            {
                TrackData(ex.Message);
                EventLog.WriteEntry(strEventLogTitle, string.Format(strExceptionMessage, ex.Message, ex.InnerException, ex.StackTrace, ex.Source), EventLogEntryType.Error, 1);
                throw;
            }
            finally
            {
                if (emg != null)
                    emg.Dispose();

            }
            return ActivityExecutionStatus.Closed;

        }

        void setupEnterpriseManagementObjects()
        {
            mpManagementPacks = emg.ManagementPacks.GetManagementPacks(
                new List<Guid>()
                {
                    new Guid("171d60e8-a0df-e4b2-f032-a5af5c8ebe39"), //Cireson Project Server Automation Library (Cireson.ProjectAutomation.Library) (171d60e8-a0df-e4b2-f032-a5af5c8ebe39)
                    new Guid("6c2bb84d-3c1f-4006-c551-6ef50179e2e3"), //System Work Item Change Request Library (System.WorkItem.ChangeRequest.Library) (6c2bb84d-3c1f-4006-c551-6ef50179e2e3)
                    new Guid("50ba92b8-f5c7-fa12-4893-0a55df43d7ce"), //System WorkItem Release Record Library (System.WorkItem.ReleaseRecord.Library) (50ba92b8-f5c7-fa12-4893-0a55df43d7ce)
                    new Guid("545131f0-58de-1914-3a82-4fcac9100a33") //Windows Core Library (Microsoft.Windows.Library) (545131f0-58de-1914-3a82-4fcac9100a33)
                });

            mpcClasses = emg.EntityTypes.GetClasses(
                new List<Guid>()
                {
                    new Guid("5a49b80c-4c34-d189-ca94-a591580f1995"), // (Cireson.ProjectAutomation.Library) (Microsoft.SystemCenter.Connector.ProjectServer.Settings) (5a49b80c-4c34-d189-ca94-a591580f1995)
                    new Guid("e786e1c7-b1fe-5b8b-ef8f-9e2dc346c44f"), //Dependent Activity (System.WorkItem.Activity.Library) (System.WorkItem.Activity.DependentActivity) (e786e1c7-b1fe-5b8b-ef8f-9e2dc346c44f)
                    new Guid("d02dc3b6-d709-46f8-cb72-452fa5e082b8"), //Release Record (System.WorkItem.ReleaseRecord.Library) (System.WorkItem.ReleaseRecord) (d02dc3b6-d709-46f8-cb72-452fa5e082b8)
                    //new Guid("d581d2d6-b6cd-b558-7ac7-db233a7c82ec"), //Project Server Connector (Cireson.ProjectAutomation.Library) (Microsoft.SystemCenter.Connector.ProjectServer) (d581d2d6-b6cd-b558-7ac7-db233a7c82ec)
                    new Guid("d2cf790e-437d-5cc4-6276-abe484c89fe6"), //Cireson Project Task Activity (Cireson.ProjectAutomation.Library) (Cireson.ProjectAutomation.ProjectTask.Activity) (d2cf790e-437d-5cc4-6276-abe484c89fe6)         
                    //new Guid("42642d4f-d342-3f1b-965c-628a0f4119e2"), //Activity (System.WorkItem.Activity.Library) (System.WorkItem.Activity) (42642d4f-d342-3f1b-965c-628a0f4119e2)
                    new Guid("dabf07ea-6047-b6d7-f8d3-e88ac1d260e9"), //Cireson Project (Cireson.ProjectAutomation.Library) (Cireson.ProjectAutomation.Project) (dabf07ea-6047-b6d7-f8d3-e88ac1d260e9)              
                    new Guid("3567434d-015f-8dcc-f188-0a407f3a2168") //Active Directory User or Group (Microsoft.Windows.Library) (Microsoft.AD.UserBase) (3567434d-015f-8dcc-f188-0a407f3a2168)
                });

            mprRelationships = emg.EntityTypes.GetRelationshipClasses(
                new List<Guid>()
                {
                    new Guid("473c9e8a-2d84-2286-36b7-17e01c9455dd"), //Depends on Work Item (System.WorkItem.Activity.Library) (System.DependentActivityDependsOnWorkItem) (473c9e8a-2d84-2286-36b7-17e01c9455dd)
                    new Guid("2da498be-0485-b2b2-d520-6ebd1698e61b"), //Contains Activity (System.WorkItem.Activity.Library) (System.WorkItemContainsActivity) (2da498be-0485-b2b2-d520-6ebd1698e61b)
                    new Guid("6f0cb41b-9f7b-f720-9c50-6d54bd56e498"), //(Cireson) WorkItem Has Project (Cireson.ProjectAutomation.Library) (Cireson.ProjectAutomation.WorkItemHasProject) (6f0cb41b-9f7b-f720-9c50-6d54bd56e498)
                    new Guid("da3123d1-2b52-a281-6f42-33d0c1f06ab4"), //Has Parent Work Item (System.WorkItem.Library) (System.WorkItemHasParentWorkItem) (da3123d1-2b52-a281-6f42-33d0c1f06ab4)
                    new Guid("15e577a3-6bf9-6713-4eac-ba5a5b7c4722"), //Assigned To User (System.WorkItem.Library) (System.WorkItemAssignedToUser) (15e577a3-6bf9-6713-4eac-ba5a5b7c4722)
                    new Guid("d96c8b59-8554-6e77-0aa7-f51448868b43"), //Is Related to Configuration Item (System.WorkItem.Library) (System.WorkItemRelatesToConfigItem) (d96c8b59-8554-6e77-0aa7-f51448868b43)
                    new Guid("cd1890b5-fd99-b732-14a3-8bb7bcd0320f") //Project Has Task (Cireson.ProjectAutomation.Library) (Cireson.ProjectAutomation.ProjectHasTask) (cd1890b5-fd99-b732-14a3-8bb7bcd0320f)
                });


            //ManagementPackTypeProjection mptpProjectTaskActivityRelatesToProject = emg.EntityTypes.GetTypeProjection("Cireson.ProjectAutomation.ProjectTaskActivityRelatesToProject.ProjectionType",
            //    mpManagementPacks.First(mp => mp.Name == "Cireson.ProjectAutomation.Library"));

            //get the settings class instance.  We'll need this later when we query Project for tasks
            emoProjectAutomationSettings = emg.EntityObjects.GetObject<EnterpriseManagementObject>(
            mpcClasses.First(mpc => mpc.Name == "Microsoft.SystemCenter.Connector.ProjectServer.Settings").Id, ObjectQueryOptions.Default);

            //get needed enumerations
            activityStatusEnums = emg.EntityTypes.GetChildEnumerations(new Guid("57db4880-000e-20bb-2f9d-fe4e8aca3cf6"), TraversalDepth.Recursive);
            releaseStatusEnums = emg.EntityTypes.GetChildEnumerations(new Guid("8909ce55-a87f-2d7e-eb64-aba670596696"), TraversalDepth.Recursive);
            //find the activity set as a placeholder in settings
            //ManagementPackEnumeration mpeInsertionStage = emoProjectAutomationSettings[null, "ActivityStageInsertionEnum"].Value as ManagementPackEnumeration;

            //get the connector instance
            emoConnector = emg.EntityObjects.GetObject<EnterpriseManagementObject>(
                (Guid)emoProjectAutomationSettings[null, "ConnectorId"].Value, ObjectQueryOptions.Default);

            //get the task activity prefix
            sTaskActivityPrefix = emoProjectAutomationSettings[null, "TaskActivityPrefix"].Value as string;
        }

        void processReleaseRecords()
        {
            ManagementPackTypeProjection mptpReleaseRecordHasProject = emg.EntityTypes.GetTypeProjection("Cireson.ProjectAutomation.ReleaseRecordHasProject.ProjectionType",
                mpManagementPacks.First(mp => mp.Name == "Cireson.ProjectAutomation.Library"));

            ObjectProjectionCriteria opcCriteria = new ObjectProjectionCriteria(xmlReleaseRecordHasProjectCriteria, mptpReleaseRecordHasProject,
                mpManagementPacks.First(mp => mp.Name == "System.WorkItem.ReleaseRecord.Library"), emg);

            //get the type projection
            var releaseRecordProjection = emg.EntityObjects.GetObjectProjectionReader<EnterpriseManagementObject>(opcCriteria, ObjectQueryOptions.Default).Where(
                p => p.Any(e => e.Value.Object.IsInstanceOf(mpcClasses.First(mpc => mpc.Name == "Cireson.ProjectAutomation.Project"))));

            if (releaseRecordProjection.Count() > 0)
                EventLog.WriteEntry(strEventLogTitle, string.Format("{0} Release Records have a Project related and are 'In-Progress' or in 'Editing' mode.  Processing Release Records.", releaseRecordProjection.Count()));
            else
                EventLog.WriteEntry(strEventLogTitle, "No Release Records have a related Project and are 'In-Progress' or in 'Editing' mode.");

            foreach(EnterpriseManagementObjectProjection emo in releaseRecordProjection)
            {
                try
                {
                    EnterpriseManagementObject emoReleaseRecord = emo.Object;
                    EnterpriseManagementObject emoProject = emo.FirstOrDefault(e => e.Value.Object.IsInstanceOf(mpcClasses.First(mpc => mpc.Name == "Cireson.ProjectAutomation.Project"))).Value.Object as EnterpriseManagementObject;

                    if (emoProject != null)
                    {
                        //log the found release record relationship
                        EventLog.WriteEntry(strEventLogTitle, string.Format(
                           "The Release Record '{0}' is in '{1}' status.  Project '{2}' is the task source. Begining sync of tasks from Project Server to the Release Record...",
                                                emoReleaseRecord.DisplayName,
                                                releaseStatusEnums.First(e => e == emoReleaseRecord[null, "Status"].Value).DisplayName,
                                                emoProject.DisplayName));

                        buildReleaseRecord(emoReleaseRecord, emoProject);
                    }
                }
                catch (EnterpriseManagementException ex)
                {
                    exceptionsList.Add(new ReleaseRecordProcessingException(string.Format("An error occured while processing '{0}'.", emo.Object.DisplayName), ex));
                }

            }

        }

        void processChangeRequests()
        {
            //get a list of projects with a change request associated to them.
            ManagementPackTypeProjection mptpChangeRequestHasProject = emg.EntityTypes.GetTypeProjection("Cireson.ProjectAutomation.ChangeRequestHasProjectAndDependentActivities.ProjectionType",
                mpManagementPacks.First(mp => mp.Name == "Cireson.ProjectAutomation.Library"));

            //set up the type projection criteria
            ObjectProjectionCriteria opcCriteria = new ObjectProjectionCriteria(xmlChangeRequestHasProjectCriteria, mptpChangeRequestHasProject,
                mpManagementPacks.First(mp => mp.Name == "System.WorkItem.ChangeRequest.Library"), emg);

            //get the type projection
            var changeRequestProjection = emg.EntityObjects.GetObjectProjectionReader<EnterpriseManagementObject>(opcCriteria, ObjectQueryOptions.Default).Where(
                p => p.Any(e => e.Value.Object.IsInstanceOf(mpcClasses.First(mpc => mpc.Name == "Cireson.ProjectAutomation.Project"))));

            //log the number of change requests processed.
            if(changeRequestProjection.Count() > 0)  
                EventLog.WriteEntry(strEventLogTitle, string.Format("{0} Change Requests have a Project and are in an 'In-Progress' status.  Processing Change Requests...", changeRequestProjection.Count()));
            else
                EventLog.WriteEntry(strEventLogTitle, string.Format("{0} Change Requests have a Project and are in an 'In-Progress' status.  Exiting workflow.", changeRequestProjection.Count()));

            foreach (EnterpriseManagementObjectProjection emo in changeRequestProjection)
            {
                try
                {
                    //get the work item and the project
                    EnterpriseManagementObject emoChangeRequest = emo.Object;
                    EnterpriseManagementObject emoProject = emo.FirstOrDefault(e => e.Value.Object.IsInstanceOf(mpcClasses.First(mpc => mpc.Name == "Cireson.ProjectAutomation.Project"))).Value.Object as EnterpriseManagementObject;

                    var dependentActivities = emo.Where(e => e.Value.Object.IsInstanceOf(mpcClasses.First(mpc => mpc.Name == "System.WorkItem.Activity.DependentActivity")));

                    //if there are dependent activities and we have a project, continue on...
                    if (dependentActivities.Count() > 0 && emoProject != null)
                    {
                        foreach (var dependentActivity in dependentActivities)
                        {
                            try
                            {
                                EnterpriseManagementObject dependsOnWorkItemActivity =
                                    emg.EntityObjects.GetRelatedObjects<EnterpriseManagementObject>(dependentActivity.Value.Object.Id,
                                        mprRelationships.First(r => r.Name == "System.DependentActivityDependsOnWorkItem"),
                                        TraversalDepth.OneLevel,
                                        ObjectQueryOptions.Default).FirstOrDefault();

                                if (dependsOnWorkItemActivity != null)
                                {
                                    //get the parent work item (release record).
                                    EnterpriseManagementObject emoReleaseRecord = emg.EntityObjects.GetParentObjects<EnterpriseManagementObject>(dependsOnWorkItemActivity.Id, ObjectQueryOptions.Default).FirstOrDefault(
                                        e => e.IsInstanceOf(mpcClasses.First(mpc => mpc.Name == "System.WorkItem.ReleaseRecord")));

                                    // only process release records that are 'In-Progress', "On-Hold', or in 'Editing' status.
                                    if (emoReleaseRecord != null && (
                                        emoReleaseRecord[null, "Status"].Value == releaseStatusEnums.First(e => e.Name == "ReleaseStatusEnum.InProgress") ||
                                        emoReleaseRecord[null, "Status"].Value == releaseStatusEnums.First(e => e.Name == "ReleaseStatusEnum.Editing") ||
                                        emoReleaseRecord[null, "Status"].Value == releaseStatusEnums.First(e => e.Name == "ReleaseStatusEnum.OnHold")))
                                    {
                                        //log the found release record relationship
                                        EventLog.WriteEntry(strEventLogTitle, string.Format(
                                            "Change Request '{0}' is related to Release Record '{1}' through a Dependent Activity.  " + "The Release Record is in '{2}' status. " +
                                            "Project '{3}' is the task source. Begining sync of tasks from Project Server to the Release Record...",
                                            emoChangeRequest.DisplayName,
                                            emoReleaseRecord.DisplayName,
                                            releaseStatusEnums.First(e => e == emoReleaseRecord[null, "Status"].Value).DisplayName,
                                            emoProject.DisplayName));

                                        buildReleaseRecord(emoReleaseRecord, emoProject);

                                    }
                                }
                            }
                            catch(EnterpriseManagementException ex)
                            {
                                exceptionsList.Add(new DependentActivityProcessingException(string.Format("An error occured while processing '{0}' related to '{1}'.", 
                                    dependentActivity.Value.Object.DisplayName, emo.Object.DisplayName), ex));
                            }
                        }
                    }
                }
                catch(EnterpriseManagementException ex)
                {
                    exceptionsList.Add(new ChangeRecordProcessingException(string.Format("An error occured while processing '{0}'", emo.Object.DisplayName), ex));
                }
            }
        }

        void buildReleaseRecord(EnterpriseManagementObject emoReleaseRecord, EnterpriseManagementObject emoProject)
        {
            try
            {
                //get the project associated with the original change request and query project server for it.
                var projectGuid = (Guid)emoProject[null, "GUID"].Value;
                projectConnector.LoadProject(projectGuid);
                //load the first level project task data
                PublishedProject project = projectConnector.Projects.FirstOrDefault(p => p.Id == projectGuid);

                if (project == null)
                    EventLog.WriteEntry(strEventLogTitle, string.Format("Project was not found on Project Server.  It may have been removed.\r\n\r\nProject Name: {0}\r\nProject GUID: {1}",
                        emoProject.DisplayName, (emoProject[null, "GUID"].Value == null) ? string.Empty : emoProject[null, "GUID"].Value.ToString()), EventLogEntryType.Warning);
                else
                {
                    projectConnector.LoadProjectTaskData(project, 1);

                    //get all activities in the release record
                    var lstReleaseRecordActivites = emg.EntityObjects.GetRelatedObjects<EnterpriseManagementObject>(emoReleaseRecord.Id,
                                                                    mprRelationships.First(r => r.Name == "System.WorkItemContainsActivity"),
                                                                    TraversalDepth.OneLevel,
                                                                    ObjectQueryOptions.Default);

                    buildActivitiesList(lstReleaseRecordActivites, emoReleaseRecord, emoProject, project);

                    EventLog.WriteEntry(strEventLogTitle, string.Format("Succsesfully processed '{0}' activities list.  Any changes will be submiited to the database.", emoReleaseRecord.DisplayName));
                }
            }
            catch(SharePoint.Client.ServerException ex)
            {
                exceptionsList.Add(new BuildReleaseRecordException(string.Format("An error occured while building '{0}' activities.  The related Project is '{1}'.", emoReleaseRecord.DisplayName, emoProject.DisplayName), ex));
            }
            
        }

        int getStartingSequenceId(IList<EnterpriseManagementObject> relatedActivities, PublishedProject project)
        {
            if (emoProjectAutomationSettings[null, "ActivityStageInsertionEnum"].Value as ManagementPackEnumeration != null)
            {

                //see if any tasks exists in the activity list.  If they do, then set the sequence value equal to the first element.
                if (relatedActivities.Any(a => a[null, "TaskGUID"] == null ? false : (Guid)a[null, "TaskGUID"].Value == project.Tasks.First().Id))
                    return (int)relatedActivities.First(a => a[null, "TaskGUID"] != null && (Guid)a[null, "TaskGUID"].Value == project.Tasks.First().Id)[null, "SequenceId"].Value;
                else
                {
                    EnterpriseManagementObject emoPlaceHolderActivity = relatedActivities.FirstOrDefault(a => a[null, "Stage"].Value ==
                        emoProjectAutomationSettings[null, "ActivityStageInsertionEnum"].Value as ManagementPackEnumeration);

                    if (emoPlaceHolderActivity == null) //no stage or the wrong stage was set... default to the end of the sequence.
                        return relatedActivities.Count;
                    else
                        return (int)emoPlaceHolderActivity[null, "SequenceId"].Value; //we want to start the task insertion at this sequence number.
                }
              
            }
            else
                //no enumeration was set as a placeholder for a sequence of activities.  Put all task activities as the end of the sequence.
                return relatedActivities.Count;

        }

        void buildActivitiesList(IList<EnterpriseManagementObject> activitiesList, EnterpriseManagementObject emoReleaseRecord, EnterpriseManagementObject emoProject, PublishedProject project)
        {

            int sequenceId = getStartingSequenceId(activitiesList, project);

            var lstEndingActivities = activitiesList.Where(
                a => (int)a[null, "SequenceId"].Value >= sequenceId && !a.IsInstanceOf(mpcClasses.First(mpc => mpc.Name == "Cireson.ProjectAutomation.Project"))).ToList();

            foreach (PublishedTask task in project.Tasks)
            {
                try
                {
                    if (activitiesList.Any(t => t[null, "TaskGUID"] != null && (Guid)t[null, "TaskGUID"].Value == task.Id))
                    {
                        //activity exists.  Get its current sequence ID and move up from there.
                        var emoTaskActivity = activitiesList.First(t => t[null, "TaskGUID"] != null && (Guid)t[null, "TaskGUID"].Value == task.Id);

                        if (string.IsNullOrEmpty((string)emoProjectAutomationSettings[null, "TaskInsertionTag"].Value))
                        {
                            iddBucket.Add(buildTaskActivity(task, emoReleaseRecord, emoTaskActivity, emoProject, ref sequenceId));
                            lstEndingActivities.Remove(emoTaskActivity);
                        }

                        else
                        {
                            if (task.Name.Contains((string)emoProjectAutomationSettings[null, "TaskInsertionTag"].Value))
                            {
                                iddBucket.Add(buildTaskActivity(task, emoReleaseRecord, emoTaskActivity, emoProject, ref sequenceId));
                                lstEndingActivities.Remove(emoTaskActivity);
                            }
                            // otherwise do nothing with this task.
                        }

                    }
                    else
                    {
                        CreatableEnterpriseManagementObject cemoProjectTaskActivity = new CreatableEnterpriseManagementObject(emg,
                            mpcClasses.First(mpc => mpc.Name == "Cireson.ProjectAutomation.ProjectTask.Activity"));

                        if (string.IsNullOrEmpty((string)emoProjectAutomationSettings[null, "TaskInsertionTag"].Value))
                            iddBucket.Add(buildTaskActivity(task, emoReleaseRecord, cemoProjectTaskActivity, emoProject, ref sequenceId));
                        else
                            if (task.Name.Contains((string)emoProjectAutomationSettings[null, "TaskInsertionTag"].Value))
                                iddBucket.Add(buildTaskActivity(task, emoReleaseRecord, cemoProjectTaskActivity, emoProject, ref sequenceId));

                    }
                }
                catch (EnterpriseManagementException ex)
                {
                    exceptionsList.Add(new ProjectTaskImportException(string.Format("An error occured while processing task '{0}'.  Unable to relate the task to '{1}'", task.Name, emoReleaseRecord.DisplayName), ex));
                }

                if (lstEndingActivities != null)
                    foreach (var activity in lstEndingActivities.OrderBy(a => (int)a[null, "SequenceId"].Value))
                        iddBucket.Add(resequenceActivity(activity, ref sequenceId));

            }
           
        }

        EnterpriseManagementObject resequenceActivity(EnterpriseManagementObject activity, ref int sequenceId)
        {
            activity[null, "SequenceId"].Value = sequenceId;

            if (activity[null, "Status"].Value == activityStatusEnums.First(e => e.Name == "ActivityStatusEnum.Active"))
                activity[null, "Status"].Value = activityStatusEnums.First(e => e.Name == "ActivityStatusEnum.Ready"); // set to pending.

            sequenceId++;
            return activity;

        }

        EnterpriseManagementObject buildTaskActivity(PublishedTask task, 
            EnterpriseManagementObject releaseRecord,
            EnterpriseManagementObject taskActivity, 
            EnterpriseManagementObject project, 
            ref int sequenceId)
        {
            var activityClass = mpcClasses.First(mpc => mpc.Name == "Cireson.ProjectAutomation.ProjectTask.Activity");

            #region set properties
            if (taskActivity.IsNew)
            {
                taskActivity[activityClass, "Id"].Value = sTaskActivityPrefix + taskActivity[activityClass, "Id"].Value as string;
                taskActivity[activityClass, "TaskGUID"].Value = task.Id;
                if (releaseRecord[null, "Status"].Value == releaseStatusEnums.First(e => e.Name == "ReleaseStatusEnum.InProgress")) //if RR in progress, set initial status to pending.
                    taskActivity[activityClass, "Status"].Value = activityStatusEnums.First(e => e.Name == "ActivityStatusEnum.Ready"); 
                else if (releaseRecord[null, "Status"].Value == releaseStatusEnums.First(e => e.Name == "ReleaseStatusEnum.Editing")) // if RR in editing mode, set initial status to on-hold.
                    taskActivity[activityClass, "Status"].Value = activityStatusEnums.First(e => e.Name == "ActivityStatusEnum.OnHold"); 
            }

            taskActivity[activityClass, "PercentComplete"].Value = task.PercentComplete as int?;
            taskActivity[activityClass, "Title"].Value = task.Name + " : " + taskActivity[activityClass, "PercentComplete"].Value + "% " + 
                mpManagementPacks.First(mp => mp.Name == "Cireson.ProjectAutomation.Library").GetStringResource("strComplete").DisplayName;
            taskActivity[activityClass, "Description"].Value = task.Notes;
            taskActivity[activityClass, "ScheduledStartDate"].Value = task.ScheduledStart;
            taskActivity[activityClass, "ScheduledEndDate"].Value = task.ScheduledFinish;
            taskActivity[activityClass, "SequenceId"].Value = sequenceId;
            taskActivity[activityClass, "DisplayName"].Value = taskActivity[activityClass, "Id"].Value;
            #endregion

            #region status enums
            //set a status
            //Activity Status (System.WorkItem.Activity.Library) (ActivityStatusEnum) (57db4880-000e-20bb-2f9d-fe4e8aca3cf6)
            //Cancelled (System.WorkItem.Activity.Library) (ActivityStatusEnum.Cancelled) (89465302-2a23-d2b6-6906-74f03d9b7b41)
            //Completed (System.WorkItem.Activity.Library) (ActivityStatusEnum.Completed) (9de908a1-d8f1-477e-c6a2-62697042b8d9)
            //Failed (System.WorkItem.Activity.Library) (ActivityStatusEnum.Failed) (144bcd52-a710-2778-2a6e-c62e0c8aae74)
            //In Progress (System.WorkItem.Activity.Library) (ActivityStatusEnum.Active) (11fc3cef-15e5-bca4-dee0-9c1155ec8d83)
            //On Hold (System.WorkItem.Activity.Library) (ActivityStatusEnum.OnHold) (d544258f-24da-1cf3-c230-b057aaa66bed)
            //Pending (System.WorkItem.Activity.Library) (ActivityStatusEnum.Ready) (50c667cf-84e5-97f8-f6f8-d8acd99f181c)
            //Rerun (System.WorkItem.Activity.Library) (ActivityStatusEnum.Rerun) (baa948b5-cc6a-57d7-4b56-d2012721b2e5)
            //Skipped (System.WorkItem.Activity.Library) (ActivityStatusEnum.Skipped) (eaec5899-b13c-d107-3e1a-955da6bf9fa7)


            if (task.PercentComplete == 100 && releaseRecord[null, "Status"].Value == releaseStatusEnums.First(e => e.Name == "ReleaseStatusEnum.InProgress")) // only set status if RR is in progress
                taskActivity[activityClass, "Status"].Value = activityStatusEnums.First(e => e.Name == "ActivityStatusEnum.Completed");
            #endregion

            #region set  workitem and CI relationships
            CreatableEnterpriseManagementRelationshipObject cemroReleaseContainsActivity = new CreatableEnterpriseManagementRelationshipObject(
                emg, mprRelationships.First(r => r.Name == "System.WorkItemContainsActivity"));
            cemroReleaseContainsActivity.SetSource(releaseRecord);
            cemroReleaseContainsActivity.SetTarget(taskActivity);
            iddBucket.Add(cemroReleaseContainsActivity);

            CreatableEnterpriseManagementRelationshipObject cemroActivityHasProject = new CreatableEnterpriseManagementRelationshipObject(
                emg, mprRelationships.First(r => r.Name == "Cireson.ProjectAutomation.WorkItemHasProject"));
            cemroActivityHasProject.SetSource(taskActivity);
            cemroActivityHasProject.SetTarget(project);
            iddBucket.Add(cemroActivityHasProject);

            CreatableEnterpriseManagementRelationshipObject cemroRelatedProject = new CreatableEnterpriseManagementRelationshipObject(
                emg, mprRelationships.First(r => r.Name == "Cireson.ProjectAutomation.ProjectHasTask"));
            cemroRelatedProject.SetSource(project);
            cemroRelatedProject.SetTarget(taskActivity);
            iddBucket.Add(cemroRelatedProject);
            #endregion

            #region set activity implemeter and resources
            var userClass = mpcClasses.First(mpc => mpc.Name == "Microsoft.AD.UserBase");

            var taskOwner = task.Assignments.FirstOrDefault();
            if (taskOwner != null && taskOwner.Resource.DefaultAssignmentOwner.ServerObjectIsNull != true) //bug fix.  We needed to check the server if the default owner is null too.
            {
                string activityImplementerSID = taskOwner.Resource.DefaultAssignmentOwner.UserId.NameId;

                EnterpriseManagementObjectCriteria emoCriteria = new EnterpriseManagementObjectCriteria(string.Format(xmlResourceCriteria, activityImplementerSID),
                    userClass, mpManagementPacks.First(mp => mp.Name == "Microsoft.Windows.Library"), emg);
                EnterpriseManagementObject emoImplementer = emg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(emoCriteria, ObjectQueryOptions.Default).FirstOrDefault();

                if (emoImplementer != null && (bool)emoProjectAutomationSettings[null, "CanAssignActivityImplementer"].Value)
                {
                    CreatableEnterpriseManagementRelationshipObject cemroActivityImplementer = new CreatableEnterpriseManagementRelationshipObject(
                           emg, mprRelationships.First(r => r.Name == "System.WorkItemAssignedToUser"));
                    cemroActivityImplementer.SetSource(taskActivity);
                    cemroActivityImplementer.SetTarget(emoImplementer);
                    iddBucket.Add(cemroActivityImplementer);
                }

                foreach (var assignment in task.Assignments)
                {
                    //get the user SID
                    if (assignment.Resource.DefaultAssignmentOwner.ServerObjectIsNull == true)
                        continue;

                    var resourceSid = assignment.Resource.DefaultAssignmentOwner.UserId.NameId;
                    emoCriteria = new EnterpriseManagementObjectCriteria(string.Format(xmlResourceCriteria, resourceSid),
                        userClass, mpManagementPacks.First(mp => mp.Name == "Microsoft.Windows.Library"), emg);
                    EnterpriseManagementObject emoResource = emg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(emoCriteria, ObjectQueryOptions.Default).FirstOrDefault();

                    if (emoResource != null)
                    {
                        CreatableEnterpriseManagementRelationshipObject cemroResource = new CreatableEnterpriseManagementRelationshipObject(
                               emg, mprRelationships.First(r => r.Name == "System.WorkItemRelatesToConfigItem"));
                        cemroResource.SetSource(taskActivity);
                        cemroResource.SetTarget(emoResource);
                        iddBucket.Add(cemroResource);
                    }


                }
            }
            #endregion

            sequenceId++;
            return taskActivity;
            

        }

        TimeSpan getRunTime(DateTime runTime)
        {
            return DateTime.UtcNow.Subtract(runTime);
        }

        string checkLicense()
        {
            IX1 x = new X1(emg, emoProjectAutomationSettings.Id, "Project Connector");
            return x.CheckLicense();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//service manager references
using Microsoft.EnterpriseManagement.UI.WpfWizardFramework;
using Microsoft.EnterpriseManagement.UI.DataModel;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess.Common;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess.DataAdapters;
using Microsoft.EnterpriseManagement.UI.Extensions.Shared;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Subscriptions;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.ServiceManager.Application.Common.CommonWizard.ConfigurationRules.Pages;
using Microsoft.EnterpriseManagement.ServiceManager.Application.Common.CommonWizard.Pages;
using Microsoft.EnterpriseManagement.ServiceManager.Application.Common.CommonWizard.ConfigurationRules;
//other references
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Globalization;
//using System.Xml;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes
{

    public class AdminSettingsData : WizardData, INotifyPropertyChanged
    {
        public IList<ManagementPackEnumeration> ActivityStageEnums {get; set;}
        public IList<EnterpriseManagementObject> ProjectConnectors { get; set; }
        
        EnterpriseManagementGroup emg;
        EnterpriseManagementObject emoSettings;
        ManagementPackRule mprGrooming;
        ManagementPackRule mprTasksAsActivities;
        ManagementPack mpProjectAutomationConfig;
        EnterpriseManagementObject selectedConnector;
        ManagementPackEnumeration activityStageInsertionEnum;
        
        int? retentionDays;
        int taskEvalFrequencyInterval = 15;
        int maxRunningTime = 120;
        bool isCompletedStatus = true;
        bool isNewStatus = false;
        bool isInProgressStatus = false;
        bool isRetentionEnabled = false;
        bool isTaskInsertionEnabled = false;
        bool isChangeProcessingEnabled = true;
        bool canAssignActivityImplementer = true;
        string taskInsertionTag = string.Empty;
        string taskActivityPrefix = "TA";
        string taskEvalFrequencyUnit = ServiceManagerLocalization.GetStringFromManagementPack("strMinutes");
        string licenseKey = "";
        static string sGroomingRuleId = "Cireson.ProjectServer.Automation.Grooming";
        static string sTasksAsActivitesRuleId = "Cireson.ProjectServer.Automation.InsertProjectTasksAsActivites";

        #region XML Criteria
        static string xmlGroomingSubscription = @"
                    <Subscription>
                        <WindowsWorkflowConfiguration>                
                            <AssemblyName>Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows</AssemblyName>       
                            <WorkflowTypeName>Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.DataGrooming</WorkflowTypeName>
                            <WorkflowParameters> 
                                <WorkflowParameter Name=""RetentionDays"" Type=""int"">{0}</WorkflowParameter>
                            </WorkflowParameters> 
                            <RetryExceptions /> 
                            <RetryDelaySeconds>60</RetryDelaySeconds> 
                            <MaximumRunningTimeSeconds>{1}</MaximumRunningTimeSeconds> 
                        </WindowsWorkflowConfiguration> 
                    </Subscription>";
        static string xmlTasksInsertionSubscription = @"<Subscription>
                        <WindowsWorkflowConfiguration>                
                            <AssemblyName>Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows</AssemblyName>       
                            <WorkflowTypeName>Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.ProcessTasksAsActivites</WorkflowTypeName>
                            <WorkflowParameters> 
                            </WorkflowParameters> 
                            <RetryExceptions /> 
                            <RetryDelaySeconds>60</RetryDelaySeconds> 
                            <MaximumRunningTimeSeconds>{0}</MaximumRunningTimeSeconds> 
                        </WindowsWorkflowConfiguration> 
                    </Subscription>";
//        static string xmlTasksAsActivitesSchedule = @"
//                              <Scheduler>
//                                 <SimpleReccuringSchedule>
//                                     <Interval Unit=""{0}"">{1}</Interval>
//                                 </SimpleReccuringSchedule>
//                                <ExcludeDates />
        //                            </Scheduler>";
        #endregion 


        #region IPropertyNotify implementation
        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged(string caller)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(caller));
        }

        public bool CanAssignActivityImplementer
        {
            get
            {
                return canAssignActivityImplementer;
            }
            set
            {
                if (canAssignActivityImplementer != value)
                {
                    canAssignActivityImplementer = value;
                    NotifyPropertyChanged("CanAssignActivityImplementer");
                }
            }
        }

        public bool IsChangeProcessingEnabled
        {
            get
            {
                return isChangeProcessingEnabled;
            }
            set
            {
                if(isChangeProcessingEnabled != value)
                {
                    isChangeProcessingEnabled = value;
                    NotifyPropertyChanged("IsChangeProcessingEnabled");
                }
            }
        }

        public EnterpriseManagementObject SelectedConnector
        {
            get
            {
                return selectedConnector;
            }
            set
            {
                if (selectedConnector != value)
                {
                    selectedConnector = value;
                    NotifyPropertyChanged("SelectedConnector");

                }
            }
        }
        public string TaskEvalFrequencyUnit
        {
            get
            {
                return taskEvalFrequencyUnit;
            }
            set
            {
                if (this.taskEvalFrequencyUnit != value)
                {
                    taskEvalFrequencyUnit = value;
                    NotifyPropertyChanged("TaskEvalFrequencyUnit");
                }
            }
        }

        public string TaskActivityPrefix
        {
            get
            {
                return taskActivityPrefix;
            }
            set
            {
                if (this.taskActivityPrefix != value)
                    taskActivityPrefix = value;
            }
        }
        public ManagementPackEnumeration ActivityStageInsertionEnum
        {
            get
            {
                return activityStageInsertionEnum;
            }

            set
            {
                if (this.activityStageInsertionEnum != value)
                {
                    activityStageInsertionEnum = value;
                    NotifyPropertyChanged("ActivityStageInsertionEnum");
                }
            }
        }

        public int TaskEvalFrequencyInterval
        {
            get
            {
                return taskEvalFrequencyInterval;
            }

            set
            {
                if (this.taskEvalFrequencyInterval != value)
                {
                    taskEvalFrequencyInterval = value;
                    NotifyPropertyChanged("TaskEvalFrequencyInterval");
                }
            }
        }

        public string TaskInsertionTag
        {
            get
            {
                return taskInsertionTag;
            }

            set
            {
                if (this.taskInsertionTag != value)
                {
                    taskInsertionTag = value;
                    NotifyPropertyChanged("TaskInsertionTag");
                }
            }

        }

        public bool IsTaskInsertionEnabled
        {
            get
            {
                return isTaskInsertionEnabled;
            }

            set
            {
                if (this.isTaskInsertionEnabled != value)
                {
                    isTaskInsertionEnabled = value;
                    NotifyPropertyChanged("IsTaskInsertionEnabled");
                }
            }

        }

        public bool IsInProgressStatus
        {
            get
            {
                return isInProgressStatus;
            }
            set
            {
                if (this.isInProgressStatus != value)
                {
                    isInProgressStatus = value;
                    NotifyPropertyChanged("IsInProgressStatus");
                }
            }
        }
        public bool IsNewStatus
        {
            get
            {
                return isNewStatus;
            }
            set
            {
                if (this.isNewStatus != value)
                {
                    isNewStatus = value;
                    NotifyPropertyChanged("IsNewStatus");
                }
            }
        }
        public bool IsCompletedStatus
        {
            get
            {
                return isCompletedStatus;
            }
            set
            {
                if (this.isCompletedStatus != value)
                {
                    isCompletedStatus = value;
                    NotifyPropertyChanged("IsCompletedStatus");
                }
            }
        }
        public bool IsRetentionEnabled
        {
            get
            {
                return isRetentionEnabled;
            }
            set
            {
                if (this.isRetentionEnabled != value)
                {
                    isRetentionEnabled = value;
                    NotifyPropertyChanged("IsEnabled");
                }
            }
        }

        public string LicenseKey
        {
            get
            {
                return licenseKey;
            }
            set
            {
                if (this.licenseKey != value)
                {
                    licenseKey = value;
                    NotifyPropertyChanged("LicenseKey");
                }
            }
        }

        public int? RetentionDays
        {
            get
            {
                return retentionDays;
            }
            set
            {
                if (retentionDays != value)
                {
                    retentionDays = value;
                    NotifyPropertyChanged("RetentionDays");
                }
            }
        }

        #endregion


        public AdminSettingsData(EnterpriseManagementObject _emoSettings)
        {
            try
            {
                emg = ConsoleContext.GetConsoleEMG();

                //get activity stage enumeration values
                ManagementPackEnumerationCriteria mpecActivityStageEnums = new ManagementPackEnumerationCriteria("Parent = 'f05ea0f0-bd02-143e-2b74-303609750328'");
                this.ActivityStageEnums = emg.EntityTypes.GetEnumerations(mpecActivityStageEnums);
                this.ActivityStageEnums = this.ActivityStageEnums.OrderBy(e => e.DisplayName).ToList();

                this.emoSettings = _emoSettings;
                //this is here for when we license this one day...
                this.LicenseKey = emoSettings[null, "Key"].Value != null ? emoSettings[null, "Key"].Value.ToString() : string.Empty;


                //set project status information.  These values have default values defined in the MP so a strict casting should be fine.
                this.IsCompletedStatus = (bool)emoSettings[null, "IsCompletedStatus"].Value;
                this.IsInProgressStatus = (bool)emoSettings[null, "IsInProgressStatus"].Value;
                this.IsNewStatus = (bool)emoSettings[null, "IsNewStatus"].Value;

                //get workflow information
                try
                {
                    this.IsRetentionEnabled = emoSettings[null, "IsRetentionEnabled"].Value == null ? false : (bool)emoSettings[null, "IsRetentionEnabled"].Value;
                    this.retentionDays = emoSettings[null, "RetentionDays"].Value as int?;

                    //Cireson Project Server Automation Library (Cireson.ProjectAutomation.Library) (171d60e8-a0df-e4b2-f032-a5af5c8ebe39)
                    //Cireson Project Server Automation Library Configuration (Cireson.ProjectAutomation.Library.Configuration) (19b2a173-bea9-9e50-0709-1470424916f2)
                    if(mpProjectAutomationConfig == null)
                        mpProjectAutomationConfig = emg.ManagementPacks.GetManagementPack("Cireson.ProjectAutomation.Library.Configuration", null, new Version("1.0.0.0"));

                    mprGrooming = mpProjectAutomationConfig.GetRule(sGroomingRuleId);


                }
                catch (ObjectNotFoundException)
                {
                    this.IsRetentionEnabled = false;
                }

                
                    this.IsTaskInsertionEnabled = emoSettings[null, "IsTaskInsertionEnabled"].Value == null ? false : (bool)emoSettings[null, "IsTaskInsertionEnabled"].Value;
                    if (this.IsTaskInsertionEnabled)
                    {

                        try
                        {
                            //project tasks section
                            this.ActivityStageInsertionEnum = (ManagementPackEnumeration)emoSettings[null, "ActivityStageInsertionEnum"].Value;
                            this.IsChangeProcessingEnabled = emoSettings[null, "IsChangeProcessingEnabled"].Value == null ? true : (bool)emoSettings[null, "IsChangeProcessingEnabled"].Value;
                            this.CanAssignActivityImplementer = emoSettings[null, "CanAssignActivityImplementer"].Value == null ? true : (bool)emoSettings[null, "CanAssignActivityImplementer"].Value;
                            this.TaskInsertionTag = emoSettings[null, "TaskInsertionTag"].Value != null ? emoSettings[null, "TaskInsertionTag"].Value.ToString() : string.Empty;
                            this.TaskActivityPrefix = emoSettings[null, "TaskActivityPrefix"].Value != null ? emoSettings[null, "TaskActivityPrefix"].Value.ToString() : "TA";
                            //read the interval from the MP rule
                            //this.EvalTasksInterval = emoSettings[null, "EvalTasksInterval"].Value as int?;


                            //Cireson Project Server Automation Library Configuration (Cireson.ProjectAutomation.Library.Configuration) (19b2a173-bea9-9e50-0709-1470424916f2)
                            if (mpProjectAutomationConfig == null)
                                mpProjectAutomationConfig = emg.ManagementPacks.GetManagementPack("Cireson.ProjectAutomation.Library.Configuration", null, new Version("1.0.0.0"));

                            mprTasksAsActivities = mpProjectAutomationConfig.GetRule(sTasksAsActivitesRuleId);

                            if(mprTasksAsActivities != null)
                            {
                                //get frequency unit and interval
                                this.TaskEvalFrequencyUnit = RulesHelper.GetFrequencyUnit(mprTasksAsActivities);

                                this.TaskEvalFrequencyInterval = RulesHelper.GetFrequencyInterval(mprTasksAsActivities);

                            }

                        }
                        catch (ObjectNotFoundException)
                        {
                            this.IsTaskInsertionEnabled = false;
                        }
      
                    }
                    //get a list of active connectors
                    // Project Server Connector (Cireson.ProjectAutomation.Library) (Microsoft.SystemCenter.Connector.ProjectServer) (d581d2d6-b6cd-b558-7ac7-db233a7c82ec)
                    ManagementPackClass mpcConnector = emg.EntityTypes.GetClass(new Guid("d581d2d6-b6cd-b558-7ac7-db233a7c82ec"));
                    ProjectConnectors = emg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(mpcConnector, ObjectQueryOptions.Default).ToList();
                    if (emoSettings[null, "ConnectorId"].Value != null)
                        SelectedConnector = ProjectConnectors.FirstOrDefault(c => c.Id == (Guid)emoSettings[null, "ConnectorId"].Value);
            }
            catch (Exception ex)
            {
                ConsoleContextHelper.Instance.ShowErrorDialog(ex, string.Empty, ConsoleFramework.ConsoleJobExceptionSeverity.Error);
            }

        }

        public override void AcceptChanges(WizardMode wizardMode)
        {
            try
            {
                //project status settings
                this.emoSettings[null, "IsNewStatus"].Value = this.IsNewStatus;
                this.emoSettings[null, "IsInProgressStatus"].Value = this.IsInProgressStatus;
                this.emoSettings[null, "IsCompletedStatus"].Value = this.IsCompletedStatus;

                //project grooming settings
                this.emoSettings[null, "Key"].Value = this.LicenseKey;
                this.emoSettings[null, "RetentionDays"].Value = this.RetentionDays;
                this.emoSettings[null, "IsRetentionEnabled"].Value = this.IsRetentionEnabled as bool?;

                setGroomingSchedule();

                //project task settings
                this.emoSettings[null, "IsTaskInsertionEnabled"].Value = this.IsTaskInsertionEnabled;
                this.emoSettings[null, "TaskInsertionTag"].Value = this.TaskInsertionTag;
                this.emoSettings[null, "TaskActivityPrefix"].Value = this.TaskActivityPrefix;
                this.emoSettings[null, "ActivityStageInsertionEnum"].Value = this.ActivityStageInsertionEnum;
                this.emoSettings[null, "CanAssignActivityImplementer"].Value = this.CanAssignActivityImplementer;
                this.emoSettings[null, "IsChangeProcessingEnabled"].Value = this.IsChangeProcessingEnabled;
                if(this.selectedConnector != null)
                    this.emoSettings[null, "ConnectorId"].Value = this.SelectedConnector.Id;


                setTaskInsertionWorkflowSchedule();


                emoSettings.Commit();

                this.WizardResult = WizardResult.Success;

            }
            catch (Exception ex)
            {
                ConsoleContextHelper.Instance.ShowErrorDialog(ex, string.Empty, ConsoleFramework.ConsoleJobExceptionSeverity.Error);
                this.WizardResult = WizardResult.Failed;
            }
        }

        private void setTaskInsertionWorkflowSchedule()
        {
            if (mprTasksAsActivities == null)
            {
                if (this.IsTaskInsertionEnabled)
                {
                    if (mpProjectAutomationConfig == null)
                        createWorkflowManagementPack();

                    //use these checks when we base a workflow off a created relationship between activities
                    //if (!mpProjectAutomationConfig.References.Contains("Activity.Library"))
                    //{
                    //    //add reference to System.WorkItem.Activity.Library aa265d90-3e2e-b9a2-d929-be0d36f1a53e
                    //    ManagementPack mpActivityLibrary = emg.ManagementPacks.GetManagementPack(new Guid("aa265d90-3e2e-b9a2-d929-be0d36f1a53e"));
                    //    ManagementPackReference mprActivityLibrary = new ManagementPackReference(mpActivityLibrary);
                    //    mpProjectAutomationConfig.References.Add("Activity.Library", mprActivityLibrary);
                    //}

                    //if (!mpProjectAutomationConfig.References.Contains("WorkItem.Library"))
                    //{
                    //    //add reference to System Work Item Library (System.WorkItem.Library) (405d5590-b45f-1c97-024f-24338290453e)
                    //    ManagementPack mpWorkItemLibrary = emg.ManagementPacks.GetManagementPack(new Guid("405d5590-b45f-1c97-024f-24338290453e"));
                    //    ManagementPackReference mprWorkItemLibrary = new ManagementPackReference(mpWorkItemLibrary);
                    //    mpProjectAutomationConfig.References.Add("WorkItem.Library", mprWorkItemLibrary);
                    //}

                    //workflow scheduling MPs
                    ManagementPack mpSystemCenter = emg.ManagementPacks.GetManagementPack(SystemManagementPack.SystemCenter);
                    ManagementPack mpSubscriptions = emg.ManagementPacks.GetManagementPack(new Guid("0306141b-bf60-70a1-be18-e979132c873c"));
                    ManagementPack mpSystem = emg.ManagementPacks.GetManagementPack(SystemManagementPack.System);

                    //rules           
                    ManagementPackDataSourceModuleType dsmScheduler = (ManagementPackDataSourceModuleType)mpSystem.GetModuleType("System.Scheduler");
                    ManagementPackWriteActionModuleType wamWorkflowTask = (ManagementPackWriteActionModuleType)mpSubscriptions.GetModuleType("Microsoft.EnterpriseManagement.SystemCenter.Subscription.WindowsWorkflowTaskWriteAction");

                    mprTasksAsActivities = new ManagementPackRule(mpProjectAutomationConfig, sTasksAsActivitesRuleId);
                    mprTasksAsActivities.Target = mpSystemCenter.GetClass("Microsoft.SystemCenter.SubscriptionWorkflowTarget");
                    ManagementPackDataSourceModule mpdsmSchedule = new ManagementPackDataSourceModule(mprTasksAsActivities, "Rule_" + sTasksAsActivitesRuleId);


                    //mpdsmSchedule.Configuration = string.Format(xmlTasksAsActivitesSchedule, this.TaskEvalFrequencyInterval);
                    //call the helper instead
                    RulesHelper.SetIntervalSchedule(ref mpdsmSchedule, this.TaskEvalFrequencyUnit, this.TaskEvalFrequencyInterval);

                    //Code for when we implement workflow kick off on relationship creation.
                    //                mpdsmSchedule.Configuration = @"<Subscription>
                    //                      <RelationshipSubscription RelType=""$MPElement[Name='Activity.Library!System.DependentActivityDependsOnWorkItem']$"" 
                    //                        SourceType=""$MPElement[Name='Activity.Library!System.WorkItem.Activity.DependentActivity']$"" 
                    //                        TargetType=""$MPElement[Name='WorkItem.Library!System.WorkItem']$"">
                    //                            <AddRelationship />
                    //                      </RelationshipSubscription>
                    //                      <PollingIntervalInSeconds>60</PollingIntervalInSeconds>
                    //                      <BatchSize>100</BatchSize>
                    //                    </Subscription>";

                    mpdsmSchedule.TypeID = dsmScheduler;
                    mprTasksAsActivities.DataSourceCollection.Add(mpdsmSchedule);

                    ManagementPackWriteActionModule writeActionModule = new ManagementPackWriteActionModule(mprTasksAsActivities, "WriteAction_" + sTasksAsActivitesRuleId);

                    writeActionModule.Configuration = string.Format(xmlTasksInsertionSubscription, maxRunningTime);
                    writeActionModule.TypeID = wamWorkflowTask;
                    mprTasksAsActivities.WriteActionCollection.Add(writeActionModule);

                    mprTasksAsActivities.Status = ManagementPackElementStatus.PendingAdd;

                    
                    mprTasksAsActivities.Enabled = ManagementPackMonitoringLevel.@true;
                   

                    mpProjectAutomationConfig.AcceptChanges();
                }
            }
            else
            {
                mprTasksAsActivities.Status = ManagementPackElementStatus.PendingUpdate;

                //use RuleScheduleHelper
                //mprTasksAsActivities.DataSourceCollection[0].Configuration = string.Format(xmlTasksAsActivitesSchedule, this.TaskEvalFrequencyInterval);
                var mpdsmSchedule = mprTasksAsActivities.DataSourceCollection[0];
                RulesHelper.SetIntervalSchedule(ref mpdsmSchedule, this.TaskEvalFrequencyUnit, this.TaskEvalFrequencyInterval);
                mprTasksAsActivities.DataSourceCollection[0] = mpdsmSchedule;

                mprTasksAsActivities.WriteActionCollection[0].Configuration = string.Format(xmlTasksInsertionSubscription, maxRunningTime);

                if (this.IsTaskInsertionEnabled)
                    mprTasksAsActivities.Enabled = ManagementPackMonitoringLevel.@true;
                else
                    mprTasksAsActivities.Enabled = ManagementPackMonitoringLevel.@false;

                mpProjectAutomationConfig.AcceptChanges();
            }
        }

        private void setGroomingSchedule()
        {

            if (this.mprGrooming == null)
            {
                if (this.IsRetentionEnabled)
                {
                    //MP is not found.  Create the MP.
                    if (mpProjectAutomationConfig == null)
                        createWorkflowManagementPack();

                    //workflow scheduling MPs
                    ManagementPack mpSystemCenter = emg.ManagementPacks.GetManagementPack(SystemManagementPack.SystemCenter);
                    ManagementPack mpSubscriptions = emg.ManagementPacks.GetManagementPack(new Guid("0306141b-bf60-70a1-be18-e979132c873c"));
                    ManagementPack mpSystem = emg.ManagementPacks.GetManagementPack(SystemManagementPack.System);

                    //rules           
                    ManagementPackDataSourceModuleType dsmScheduler = (ManagementPackDataSourceModuleType)mpSystem.GetModuleType("System.Scheduler");
                    ManagementPackWriteActionModuleType wamWorkflowTask = (ManagementPackWriteActionModuleType)mpSubscriptions.GetModuleType("Microsoft.EnterpriseManagement.SystemCenter.Subscription.WindowsWorkflowTaskWriteAction");

                    mprGrooming = new ManagementPackRule(mpProjectAutomationConfig, sGroomingRuleId);
                    mprGrooming.Target = mpSystemCenter.GetClass("Microsoft.SystemCenter.SubscriptionWorkflowTarget");
                    ManagementPackDataSourceModule mpdsmSchedule = new ManagementPackDataSourceModule(mprGrooming, "Rule_" + sGroomingRuleId);

                    mpdsmSchedule.Configuration = @"
                              <Scheduler>
                                 <SimpleReccuringSchedule>
                                     <Interval Unit=""Hours"">24</Interval>
                                 </SimpleReccuringSchedule>
                                <ExcludeDates />
                            </Scheduler>";

                    mpdsmSchedule.TypeID = dsmScheduler;
                    mprGrooming.DataSourceCollection.Add(mpdsmSchedule);

                    ManagementPackWriteActionModule writeActionModule = new ManagementPackWriteActionModule(mprGrooming, "WriteAction_" + sGroomingRuleId);

                    writeActionModule.Configuration = String.Format(xmlGroomingSubscription, this.RetentionDays, maxRunningTime);

                    writeActionModule.TypeID = wamWorkflowTask;
                    mprGrooming.WriteActionCollection.Add(writeActionModule);

                    mprGrooming.Status = ManagementPackElementStatus.PendingAdd;
                    mprGrooming.Enabled = ManagementPackMonitoringLevel.@true;

                    mpProjectAutomationConfig.AcceptChanges();
                }
            }
            else
            {
                mprGrooming.Status = ManagementPackElementStatus.PendingUpdate;
                mprGrooming.WriteActionCollection[0].Configuration = string.Format(xmlGroomingSubscription, this.RetentionDays, maxRunningTime);

                if (this.IsRetentionEnabled)
                    mprGrooming.Enabled = ManagementPackMonitoringLevel.@true;
                else
                    mprGrooming.Enabled = ManagementPackMonitoringLevel.@false;

                mpProjectAutomationConfig.AcceptChanges();

            }

        }

        private void createWorkflowManagementPack()
        {
            mpProjectAutomationConfig = new ManagementPack("Cireson.ProjectAutomation.Library.Configuration", "Cireson Project Server Automation Workflows", new Version("1.0.0.0"), emg);
            emg.ManagementPacks.ImportManagementPack(mpProjectAutomationConfig);

        }
    }

    public class ProjectConnectorData : WizardData, INotifyPropertyChanged
    {
        static string xmlSubscription = @"
                    <Subscription>
                        <WindowsWorkflowConfiguration>                
                            <AssemblyName>Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows</AssemblyName>       
                            <WorkflowTypeName>Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.ProcessProjectCIs</WorkflowTypeName>
                            <WorkflowParameters> 
                                <WorkflowParameter Name=""ConnectorId"" Type=""guid"">{0}</WorkflowParameter>
                            </WorkflowParameters> 
                            <RetryExceptions /> 
                            <RetryDelaySeconds>60</RetryDelaySeconds> 
                            <MaximumRunningTimeSeconds>{1}</MaximumRunningTimeSeconds> 
                        </WindowsWorkflowConfiguration> 
                    </Subscription>";

        //INotifyPropertyChanged
        string name;
        string pwaUrl;
        bool isCredentialsChanged;
        string resultsMessage;

        public bool IsEditMode { get; set; }
        public string Description { get; set; }
        public string RunAsAccount { get; set; }
        public Guid Id { get; set; }
        public IDataItem Connector { get; set; }

        //schedule fields
        bool isSunday = false;
        bool isMonday = false;
        bool isTuesday = false;
        bool isWednesday = false;
        bool isThursday = false;
        bool isFriday = false;
        bool isSaturday = false;
        bool isDailySchedule = true;
        bool isFrequencySchedule = false;
        string scheduledTime = string.Empty;
        string frequencyUnit = string.Empty;
        int frequencyInterval = 0;
        [Flags]
        enum daysOfWeek : int
        {
            Sunday = 1,
            Monday = 2,
            Tuesday = 4,
            Wednesday = 8,
            Thursday = 16,
            Friday = 32,
            Saturday = 64
        }
        bool isEnabled = false;
        bool isO365 = false;
        int maxRunningTime = 7200;


        //public string EncryptedCredentails { get; set; }

        #region IPropertyNotify implementation
        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged(string caller)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(caller));
        }

        public bool IsO365
        {
            get
            {
                return isO365;
            }
            set
            {
                if(this.isO365 != value)
                {
                    this.isO365 = value;
                    NotifyPropertyChanged("IsO365");
                }
            }
        }

        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                if(this.isEnabled != value)
                {
                    this.isEnabled = value;
                    NotifyPropertyChanged("IsEnabled");
                }
            }
        }

        public int FrequencyInterval
        {
            get
            {
                return frequencyInterval;
            }
            set
            {
                if (this.frequencyInterval != value)
                {
                    this.frequencyInterval = value;
                    NotifyPropertyChanged("FrequencyInterval");
                }
            }
        } 
        public string FrequencyUnit
        {
            get
            {
                return frequencyUnit;
            }
            set
            {
                if (this.frequencyUnit != value)
                {
                    this.frequencyUnit = value;
                    NotifyPropertyChanged("FrequencyUnit");
                }
            }
        } 
        public string ScheduledTime
        {
            get
            {
                return scheduledTime;
            }
            set
            {
                if (this.scheduledTime != value)
                {
                    this.scheduledTime = value;
                    NotifyPropertyChanged("ScheduledTime");
                }
            }
        }

        public bool IsFrequencySchedule
        {
            get
            {
                return isFrequencySchedule;
            }
            set
            {
                if (this.isFrequencySchedule != value)
                {
                    this.isFrequencySchedule = value;
                    NotifyPropertyChanged("IsFrequencySchedule");
                }
            }
        } 
        public bool IsDailySchedule
        {
            get
            {
                return isDailySchedule;
            }
            set
            {
                if (this.isDailySchedule != value)
                {
                    this.isDailySchedule = value;
                    NotifyPropertyChanged("IsDailySchedule");
                }
            }
        } 

        public bool IsSaturday
        {
            get
            {
                return isSaturday;
            }
            set
            {
                if (this.isSaturday != value)
                {
                    this.isSaturday = value;
                    NotifyPropertyChanged("IsSaturday");
                }
            }
        }
        public bool IsFriday
        {
            get
            {
                return isFriday;
            }
            set
            {
                if (this.isFriday != value)
                {
                    this.isFriday = value;
                    NotifyPropertyChanged("IsFriday");
                }
            }
        }
        public bool IsThursday
        {
            get
            {
                return isThursday;
            }
            set
            {
                if (this.isThursday != value)
                {
                    this.isThursday = value;
                    NotifyPropertyChanged("IsThursday");
                }
            }
        }
        public bool IsWednesday
        {
            get
            {
                return isWednesday;
            }
            set
            {
                if (this.isWednesday != value)
                {
                    this.isWednesday = value;
                    NotifyPropertyChanged("IsWednesday");
                }
            }
        }
        public bool IsTuesday
        {
            get
            {
                return isTuesday;
            }
            set
            {
                if (this.isTuesday != value)
                {
                    this.isTuesday = value;
                    NotifyPropertyChanged("IsTuesday");
                }
            }
        }
        public bool IsMonday
        {
            get
            {
                return isMonday;
            }
            set
            {
                if (this.isMonday != value)
                {
                    this.isMonday = value;
                    NotifyPropertyChanged("IsMonday");
                }
            }
        }
        public bool IsSunday
        {
            get
            {
                return isSunday;
            }
            set
            {
                if(this.isSunday != value)
                {
                    this.isSunday = value;
                    NotifyPropertyChanged("IsSunday");
                }
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    NotifyPropertyChanged("Name");
                }

            }
        }
        public string PwaUrl
        {
            get
            {
                return pwaUrl;
            }
            set
            {
                if (this.pwaUrl != value)
                {
                    this.pwaUrl = value;
                    NotifyPropertyChanged("PwaUrl");
                }

            }
        }
        public bool IsCredentialsChanged
        {
            get
            {
                return isCredentialsChanged;
            }
            set
            {
                if (this.isCredentialsChanged != value)
                {
                    this.isCredentialsChanged = value;
                    NotifyPropertyChanged("IsCredentialsChanged");
                }

            }
        }
        public string ResultsMessage
        {
            get
            {
                return resultsMessage;
            }
            set
            {
                if (this.resultsMessage != value)
                {
                    this.resultsMessage = value;
                    NotifyPropertyChanged("ResultsMessage");
                }

            }
        }
        #endregion

        public ProjectConnectorData()
        {
            IsEditMode = false;
            Id = Guid.NewGuid();
            IsFrequencySchedule = false;
            IsDailySchedule = true;
        }

        public ProjectConnectorData(IDataItem data)
        {
            IsEditMode = true;
            Connector = data;
            EnterpriseManagementGroup emg = ConsoleContext.GetConsoleEMG();
            EnterpriseManagementObject emoConnector = emg.EntityObjects.GetObject<EnterpriseManagementObject>((Guid)data["$Id$"], ObjectQueryOptions.Default);
            Name = emoConnector[null, "Name"].Value.ToString();
            Description = emoConnector[null, "Description"].Value != null ? emoConnector[null, "Description"].Value.ToString() : string.Empty;
            PwaUrl = emoConnector[null, "ProjectServerURL"].Value.ToString();
            RunAsAccount = emoConnector[null, "RunAsAccount"].Value != null ? emoConnector[null, "RunAsAccount"].Value.ToString() : string.Empty;
            Id = new Guid(emoConnector[null, "Id"].Value as string);
            IsFrequencySchedule = emoConnector[null, "IsFrequencySchedule"].Value != null ? (bool)emoConnector[null, "IsFrequencySchedule"].Value : false;
            IsDailySchedule = emoConnector[null, "IsFrequencySchedule"].Value != null ? !(bool)emoConnector[null, "IsFrequencySchedule"].Value : true;
            IsEnabled = (bool)emoConnector[null, "Enabled"].Value;


            //get workflow schedule
            //Cireson Project Server Automation Library Configuration (Cireson.ProjectAutomation.Library.Configuration) (19b2a173-bea9-9e50-0709-1470424916f2)
            ManagementPack mpConnectorWorkflow = emg.ManagementPacks.GetManagementPack("Cireson.ProjectAutomation.Library.Configuration", null, new Version("1.0.0.0"));
            string sConnectorRuleId = string.Format("{0}.{1}", "Cireson.ProjectServer.Automation", new Guid(emoConnector[null, "Id"].ToString()).ToString("N"));
            ManagementPackRule mprConnector = mpConnectorWorkflow.GetRule(sConnectorRuleId);

            if (IsFrequencySchedule)
            {
                //frequency
                FrequencyInterval = RulesHelper.GetFrequencyInterval(mprConnector);
                FrequencyUnit = RulesHelper.GetFrequencyUnit(mprConnector);
            }
            else
            {
                //set days
                int daysMask = RulesHelper.GetDailySchedule(mprConnector);
                if (((daysOfWeek)(daysMask) & daysOfWeek.Sunday) == daysOfWeek.Sunday)
                    IsSunday = true;
                if (((daysOfWeek)(daysMask) & daysOfWeek.Monday) == daysOfWeek.Monday)
                    IsMonday = true;
                if (((daysOfWeek)(daysMask) & daysOfWeek.Tuesday) == daysOfWeek.Tuesday)
                    IsTuesday = true;
                if (((daysOfWeek)(daysMask) & daysOfWeek.Wednesday) == daysOfWeek.Wednesday)
                    IsWednesday = true;
                if (((daysOfWeek)(daysMask) & daysOfWeek.Thursday) == daysOfWeek.Thursday)
                    IsThursday = true;
                if (((daysOfWeek)(daysMask) & daysOfWeek.Friday) == daysOfWeek.Friday)
                    IsFriday = true;
                if (((daysOfWeek)(daysMask) & daysOfWeek.Saturday) == daysOfWeek.Saturday)
                    IsSaturday = true;

                //convert xml start time to a readable version
                DateTime dt = DateTime.Parse("01/01/2010 " + RulesHelper.GetDailyScheduleStartTime(mprConnector), System.Globalization.CultureInfo.CurrentUICulture);
                //ScheduledTime = dt.TimeOfDay.Hours.ToString().PadLeft(2, '0') + ":" + dt.TimeOfDay.Minutes.ToString().PadLeft(2, '0');
                ScheduledTime = dt.ToShortTimeString();
            }

        }

        public override void AcceptChanges(WizardMode wizardMode)
        {
            try
            {
                

                if(!IsEditMode)
                {
                    createConnectorInstance();
                }
                else
                {
                    updateConnectorInstance();
                }


                ResultsMessage = ServiceManagerLocalization.GetStringFromManagementPack("strResultSuccess");
                this.WizardResult = WizardResult.Success;
            }
            catch(Exception ex)
            {
                ResultsMessage = ex.Message;
                this.WizardResult = WizardResult.Failed;
                
            }
        }

        void createConnectorInstance()
        {
            try
            {

                #region Build connector/status object.
                EnterpriseManagementGroup emg = ConsoleContext.GetConsoleEMG();
                //Project Server Connector (Cireson.ProjectAutomation.Library) (Microsoft.SystemCenter.Connector.ProjectServer) (d581d2d6-b6cd-b558-7ac7-db233a7c82ec)
                ManagementPackClass mpcConnector = emg.EntityTypes.GetClass(new Guid("d581d2d6-b6cd-b558-7ac7-db233a7c82ec"));
                ManagementPackClass mpcStatus = emg.EntityTypes.GetClass(new Guid("2d4afd51-d2ff-92c6-266f-2b6060000dae"));

                CreatableEnterpriseManagementObject cemoConnectorStatus = new CreatableEnterpriseManagementObject(emg, mpcStatus);
                CreatableEnterpriseManagementObject cemoConnector = new CreatableEnterpriseManagementObject(emg, mpcConnector);

                string sConnectorRuleId = string.Format("{0}.{1}", "Cireson.ProjectServer.Automation", Id.ToString("N"));

                cemoConnector[null, "Name"].Value = Name;
                cemoConnector[null, "DisplayName"].Value = Name;
                cemoConnector[null, "Description"].Value = Description;
                cemoConnector[null, "ProjectServerURL"].Value = PwaUrl;
                cemoConnector[null, "RunAsAccount"].Value = RunAsAccount;
                cemoConnector[null, "DataProviderDisplayName"].Value = "Cireson Project Server Automation";
                cemoConnector[null, "Id"].Value = Id.ToString();
                cemoConnector[null, "Enabled"].Value = IsEnabled;
                cemoConnector[null, "IsFrequencySchedule"].Value = IsFrequencySchedule;
                
                //status
                cemoConnectorStatus[mpcConnector, "Id"].Value = Id.ToString();
                //Never Run (ServiceManager.LinkingFramework.Library) (Microsoft.SystemCenter.LinkingFramework.SyncStatusEnum.NeverRun) (78dcd080-5d8c-d366-40e0-e9357fbe4de4)
                ManagementPackEnumeration mpeNeverRun = emg.EntityTypes.GetEnumeration(new Guid("78dcd080-5d8c-d366-40e0-e9357fbe4de4"));
                cemoConnectorStatus[mpcStatus, "Status"].Value = mpeNeverRun;

                #endregion

                #region Build Schedule Workflow
                //workflow scheduling MPs
                ManagementPack mpSystemCenter = emg.ManagementPacks.GetManagementPack(SystemManagementPack.SystemCenter);
                ManagementPack mpSubscriptions = emg.ManagementPacks.GetManagementPack(new Guid("0306141b-bf60-70a1-be18-e979132c873c"));
                ManagementPack mpSystem = emg.ManagementPacks.GetManagementPack(SystemManagementPack.System);
                
                //Cireson Project Server Automation Library (Cireson.ProjectAutomation.Library) (171d60e8-a0df-e4b2-f032-a5af5c8ebe39)
                //Cireson Project Server Automation Library Configuration (Cireson.ProjectAutomation.Library.Configuration) (19b2a173-bea9-9e50-0709-1470424916f2)
                //check if management pack exists.  If not, create one.

                ManagementPack mpConnectorWorkflow;
                try
                {
                    mpConnectorWorkflow = emg.ManagementPacks.GetManagementPack("Cireson.ProjectAutomation.Library.Configuration", null, new Version("1.0.0.0"));
                }
                catch(ObjectNotFoundException)
                {
                    mpConnectorWorkflow = new ManagementPack("Cireson.ProjectAutomation.Library.Configuration", "Cireson Project Server Automation Workflows", new Version("1.0.0.0"), emg);
                    emg.ManagementPacks.ImportManagementPack(mpConnectorWorkflow);
                }

                //rules
                ManagementPackDataSourceModuleType dsmScheduler = (ManagementPackDataSourceModuleType)mpSystem.GetModuleType("System.Scheduler");
                ManagementPackWriteActionModuleType wamWorkflowTask = (ManagementPackWriteActionModuleType)mpSubscriptions.GetModuleType("Microsoft.EnterpriseManagement.SystemCenter.Subscription.WindowsWorkflowTaskWriteAction");
                
                ManagementPackRule mprConnector = new ManagementPackRule(mpConnectorWorkflow, sConnectorRuleId);
                mprConnector.Target = mpSystemCenter.GetClass("Microsoft.SystemCenter.SubscriptionWorkflowTarget");
                ManagementPackDataSourceModule mpdsmSchedule = new ManagementPackDataSourceModule(mprConnector, "Rule_" + sConnectorRuleId);

                if (this.IsEnabled)
                    mprConnector.Enabled = ManagementPackMonitoringLevel.@false;
                else
                    mprConnector.Enabled = ManagementPackMonitoringLevel.@true;

                if (this.IsDailySchedule)
                {
                    int syncMask = 0;
                    if (this.IsSaturday)
                        syncMask += 64;
                    if (this.IsFriday)
                        syncMask += 32;
                    if (this.IsThursday)
                        syncMask += 16;
                    if (this.IsWednesday)
                        syncMask += 8;
                    if (this.IsTuesday)
                        syncMask += 4;
                    if (this.IsMonday)
                        syncMask += 2;
                    if (this.IsSunday)
                        syncMask += 1;

                    RulesHelper.SetDailySchedule(ref mpdsmSchedule, this.ScheduledTime, syncMask.ToString());
                }
                else
                {
                    RulesHelper.SetIntervalSchedule(ref mpdsmSchedule, this.FrequencyUnit, this.FrequencyInterval);
                }

                mpdsmSchedule.TypeID = dsmScheduler;
                mprConnector.DataSourceCollection.Add(mpdsmSchedule);

                ManagementPackWriteActionModule writeActionModule = new ManagementPackWriteActionModule(mprConnector, "WriteAction_" + sConnectorRuleId);

                writeActionModule.Configuration = String.Format(xmlSubscription, cemoConnector.Id, maxRunningTime);

                writeActionModule.TypeID = wamWorkflowTask;
                mprConnector.WriteActionCollection.Add(writeActionModule);

                mprConnector.Status = ManagementPackElementStatus.PendingAdd;

                if (IsEnabled)
                    mprConnector.Enabled = ManagementPackMonitoringLevel.@true;
                else
                    mprConnector.Enabled = ManagementPackMonitoringLevel.@false;

                #endregion

                cemoConnector.Commit();
                cemoConnectorStatus.Commit();
                mpConnectorWorkflow.AcceptChanges();
            }
            catch(Exception ex)
            {
                this.ResultsMessage = ex.Message + "\n\n" + ex.GetType().ToString() + "\n\n" + ex.StackTrace;
                throw;
            }
        }

        void updateConnectorInstance()
        {

            try
            {
                #region Modify connector object.
                EnterpriseManagementGroup emg = ConsoleContext.GetConsoleEMG();

                EnterpriseManagementObject emoConnector = emg.EntityObjects.GetObject<EnterpriseManagementObject>((Guid)Connector["$Id$"], ObjectQueryOptions.Default);

                emoConnector[null, "Name"].Value = Name;
                emoConnector[null, "Description"].Value = Description;
                emoConnector[null, "ProjectServerURL"].Value = PwaUrl;
                emoConnector[null, "RunAsAccount"].Value = RunAsAccount;
                emoConnector[null, "IsFrequencySchedule"].Value = IsFrequencySchedule;
                emoConnector[null, "Enabled"].Value = IsEnabled;
                #endregion

                #region Modify Workflow Schedule
                //workflow scheduling
                ManagementPackClass mpcConnector = emg.EntityTypes.GetClass(new Guid("d581d2d6-b6cd-b558-7ac7-db233a7c82ec"));
                //Cireson Project Server Automation Library Configuration (Cireson.ProjectAutomation.Library.Configuration) (19b2a173-bea9-9e50-0709-1470424916f2)
                ManagementPack mpConnectorWorkflow = emg.ManagementPacks.GetManagementPack("Cireson.ProjectAutomation.Library.Configuration", null, new Version("1.0.0.0"));
                string sConnectorRuleId = string.Format("{0}.{1}", "Cireson.ProjectServer.Automation", new Guid(emoConnector[mpcConnector, "Id"].ToString()).ToString("N"));
                ManagementPackRule mprConnector = mpConnectorWorkflow.GetRule(sConnectorRuleId);
                ManagementPackDataSourceModule dsmSchedule = mprConnector.DataSourceCollection[0];

                if (this.IsDailySchedule)
                {
                    int syncMask = 0;
                    if (this.IsSaturday)
                        syncMask += 64;
                    if (this.IsFriday)
                        syncMask += 32;
                    if (this.IsThursday)
                        syncMask += 16;
                    if (this.IsWednesday)
                        syncMask += 8;
                    if (this.IsTuesday)
                        syncMask += 4;
                    if (this.IsMonday)
                        syncMask += 2;
                    if (this.IsSunday)
                        syncMask += 1;

                    RulesHelper.SetDailySchedule(ref dsmSchedule, this.ScheduledTime, syncMask.ToString());

                }
                else
                {
                    RulesHelper.SetIntervalSchedule(ref dsmSchedule, this.FrequencyUnit, this.FrequencyInterval);
                }

                mprConnector.WriteActionCollection[0].Configuration = string.Format(xmlSubscription, emoConnector.Id.ToString(), maxRunningTime);

                mprConnector.Status = ManagementPackElementStatus.PendingUpdate;

                if (IsEnabled)
                    mprConnector.Enabled = ManagementPackMonitoringLevel.@true;
                else
                    mprConnector.Enabled = ManagementPackMonitoringLevel.@false;

                #endregion

                emoConnector.Commit();
                mpConnectorWorkflow.AcceptChanges();

            }
            catch (Exception ex)
            {
                ConsoleContextHelper.Instance.ShowErrorDialog(ex, ServiceManagerLocalization.GetStringFromManagementPack("strConnectorTitle"), ConsoleFramework.ConsoleJobExceptionSeverity.Error);
            }
        }

        
    }   
    
}

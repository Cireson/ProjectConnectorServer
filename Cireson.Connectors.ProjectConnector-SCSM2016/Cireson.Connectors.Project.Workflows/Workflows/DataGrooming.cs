
// Copyright (C) 2019 Cireson, LLC. All Rights Reserved

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
//other references
using System.Diagnostics;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.Classes.Licensing;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.Licensing;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows
{

    public partial class DataGrooming : WorkflowActivityBase
    {
        EnterpriseManagementGroup emg;
        List<Exception> exceptionsList;
        static string strEventLogTitle = "Project Connector Grooming Workflow";
        static string strExceptionMessage = "Message: {0}\r\nInner Exception: {1}\r\nStack Trace: {2}\r\nSource: {3}";

        public static DependencyProperty RetentionDaysProperty = DependencyProperty.Register("RetentionDays", typeof(int), typeof(DataGrooming));

        public int RetentionDays
        {
            get
            {
                return (int)this.GetValue(RetentionDaysProperty);
            }
            set
            {
                this.SetValue(RetentionDaysProperty, value);
            }
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            try
            {
                if (RetentionDays == 0)
                    throw new ArgumentNullException("No retention days set.");

                //setup event log access
                if (!EventLog.SourceExists(strEventLogTitle))
                    EventLog.CreateEventSource(strEventLogTitle, "Operations Manager");

                EventLog.WriteEntry(strEventLogTitle, "Starting Project CI grooming...");

                emg = new EnterpriseManagementGroup("localhost");

                var licenseExpiration = checkLicense(); //method will throw an exception if no license exists.
                EventLog.WriteEntry(strEventLogTitle, string.Format("License Expiration: {0}", licenseExpiration));

                //Cireson Project (Cireson.ProjectAutomation.Library) (Cireson.ProjectAutomation.Project) (dabf07ea-6047-b6d7-f8d3-e88ac1d260e9)
                ManagementPackClass mpcProject = emg.EntityTypes.GetClass(new Guid("dabf07ea-6047-b6d7-f8d3-e88ac1d260e9"));

                var oldProjects = emg.EntityObjects.GetObjectReader<EnterpriseManagementObject>(new EnterpriseManagementObjectCriteria(
                    "LastModified < '" + DateTime.Now.AddDays(-RetentionDays).ToUniversalTime() + "'", mpcProject), ObjectQueryOptions.Default);
                IncrementalDiscoveryData iddDeletePending = new IncrementalDiscoveryData();

                EventLog.WriteEntry("Checking for Project CIs with a last modified date older than: {0}", DateTime.Now.AddDays(-RetentionDays).ToUniversalTime().ToString());

                //get PendingDelete enum
                //Pending Delete (System.Library) (System.ConfigItem.ObjectStatusEnum.PendingDelete) (47101e64-237f-12c8-e3f5-ec5a665412fb)
                var pendingDelete = emg.EntityTypes.GetEnumeration(new Guid("47101e64-237f-12c8-e3f5-ec5a665412fb"));

                exceptionsList = new List<Exception>();

                foreach (EnterpriseManagementObject project in oldProjects)
                {
                    try
                    {
                        project[null, "ObjectStatus"].Value = pendingDelete;
                        iddDeletePending.Add(project);
                    }
                    catch (EnterpriseManagementException ex)
                    {
                        exceptionsList.Add(new ProjectCIGroomingException(string.Format("Failed to set 'Pending Delete' object status on {0}.", project.Name), ex));
                    }
                }

                iddDeletePending.Overwrite(emg);

                if (exceptionsList.Count > 0)
                    throw new AggregateException("One or more errors occured grooming Project CIs.", exceptionsList);

                
            }
            catch (AggregateException ex)
            {
                TrackData(ex.Message);

                StringBuilder exceptionStrings = new StringBuilder();
                foreach (var innerEx in ex.InnerExceptions)
                    exceptionStrings.AppendLine(string.Format(strExceptionMessage, innerEx.Message, innerEx.InnerException, innerEx.StackTrace, innerEx.Source));

                EventLog.WriteEntry(strEventLogTitle, string.Format(ex.Message + "\r\n\r\n" + "The following projects failed:\r\n\r\n" + exceptionStrings.ToString()));
  
            }
            catch (EnterpriseManagementException ex)
            {
                TrackData(ex);
                TrackData(ex.Message);
                throw;
            }
            finally
            {
                if (emg != null)
                    emg.Dispose();
            }

            return ActivityExecutionStatus.Closed;
        }

        string checkLicense()
        {
            IX1 x = new X1(emg, new Guid("5a49b80c-4c34-d189-ca94-a591580f1995"), "Project Connector");
            return x.CheckLicense();
        }
    }
}

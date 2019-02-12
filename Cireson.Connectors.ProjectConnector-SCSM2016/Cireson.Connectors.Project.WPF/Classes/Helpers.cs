
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
//service manager references
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.ConsoleFramework;
using Microsoft.EnterpriseManagement.UI.Core;
using Microsoft.EnterpriseManagement.UI.Core.Connection;
using Microsoft.EnterpriseManagement.UI.WpfWizardFramework;
using Microsoft.EnterpriseManagement.UI.Extensions.Shared;
using Microsoft.EnterpriseManagement.UI.DataModel;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.ConnectorFramework;
//other references
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms.Integration;
using System.Security.Cryptography;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Forms;
using System.Xml;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes
{
    public class RulesHelper
    {
        #region schedule functions
        internal static void SetDailySchedule(ref ManagementPackDataSourceModule module, string time, string mask)
        {
            try
            {
                DateTime dt = DateTime.Parse("01/01/2010 " + time, System.Globalization.CultureInfo.CurrentUICulture);
                string sTime = dt.TimeOfDay.Hours.ToString() + ":" + dt.TimeOfDay.Minutes.ToString();

                string scheduleXML = string.Format(@"
                            <Scheduler>
                              <WeeklySchedule>
                                <Windows>
                                  <Daily>
                                    <Start>{0}</Start>
                                    <End>{0}</End>
                                    <DaysOfWeekMask>{1}</DaysOfWeekMask>
                                  </Daily>
                                </Windows>
                              </WeeklySchedule>
                              <ExcludeDates />
                            </Scheduler>", sTime, mask);

                module.Configuration = scheduleXML;
            }
            catch
            { }
        }

        internal static void SetIntervalSchedule(ref ManagementPackDataSourceModule module, string unit, int interval)
        {
            try
            {
                if (unit == ServiceManagerLocalization.GetStringFromManagementPack("strHours"))
                    unit = "Hours";
                else
                    unit = "Minutes";

                string sScheduleXML = String.Format(@"
                              <Scheduler>
                                 <SimpleReccuringSchedule>
                                     <Interval Unit=""{0}"">{1}</Interval>
                                 </SimpleReccuringSchedule>
                                <ExcludeDates />
                            </Scheduler>", unit, interval);
                module.Configuration = sScheduleXML;
            }
            catch { }
        }

        internal static string GetDailyScheduleStartTime(ManagementPackRule mpr)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(mpr.DataSourceCollection[0].Configuration);

                foreach (XmlNode node in xmlDoc.ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0])
                    if (node.Name == "Start")
                        return node.InnerXml;
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        internal static int GetDailySchedule(ManagementPackRule mpr)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(mpr.DataSourceCollection[0].Configuration);

                foreach (XmlNode node in xmlDoc.ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0])
                    if (node.Name == "DaysOfWeekMask")
                        return Convert.ToInt32(node.InnerXml);
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        internal static string GetFrequencyUnit(ManagementPackRule mpr)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(mpr.DataSourceCollection[0].Configuration);

                foreach (XmlNode node in xmlDoc.ChildNodes[0].ChildNodes[0])
                    if (node.Name == "Interval")
                    {
                        if (node.Attributes["Unit"].Value.ToString() == "Hours")
                            return ServiceManagerLocalization.GetStringFromManagementPack("strHours");
                        else
                            return ServiceManagerLocalization.GetStringFromManagementPack("strMinutes");
                    }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        internal static int GetFrequencyInterval(ManagementPackRule mpr)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(mpr.DataSourceCollection[0].Configuration);

                foreach (XmlNode node in xmlDoc.ChildNodes[0].ChildNodes[0])
                    if (node.Name == "Interval")
                        return Convert.ToInt32(node.InnerXml);
                return 0;
            }
            catch
            {
                return 0;
            }
        }
        #endregion
    }
    public class PasswordHepler
    {
        internal static string ConvertSecureStringToString(SecureString value)
        {
            string result;
            try
            {
                IntPtr ptr = Marshal.SecureStringToBSTR(value);
                try
                {
                    result = Marshal.PtrToStringBSTR(ptr);
                }
                finally
                {
                    Marshal.FreeBSTR(ptr);
                }
            }
            finally
            {
            }
            return result;
        }
        //borrowed from SMA connector.
        internal static string SetFullString(string text, string st, string ss)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            byte[] salt = UTF8Encoding.ASCII.GetBytes(st);

            try
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(ss, salt);
                using(RijndaelManaged rij = new RijndaelManaged { Key = pdb.GetBytes(32), IV = pdb.GetBytes(16) })
                {
                    using (var memStream = new MemoryStream())
                    using (var cryptSteam = new CryptoStream(memStream, rij.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] data = Encoding.UTF8.GetBytes(text);
                        cryptSteam.Write(data, 0, data.Length);
                        cryptSteam.FlushFinalBlock();
                        return Convert.ToBase64String(memStream.GetBuffer(), 0, (int)memStream.Length);
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetFullString(string text, string st, string ss)
        {
            if (!string.IsNullOrEmpty(text))
            {
                byte[] salt = UTF8Encoding.ASCII.GetBytes(st);

                try
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(ss, salt);
                    using (RijndaelManaged rij = new RijndaelManaged { Padding = PaddingMode.Zeros, Key = pdb.GetBytes(32), IV = pdb.GetBytes(16) })
                    {
                        using (var memStream = new MemoryStream())
                        {
                            using (var cryptStream = new CryptoStream(memStream, rij.CreateDecryptor(), CryptoStreamMode.Write))
                            {
                                byte[] data = Convert.FromBase64String(text);
                                cryptStream.Write(data, 0, data.Length);
                                cryptStream.Flush();
                                return Encoding.UTF8.GetString(memStream.ToArray());
                            }
                        }
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
            else
                return string.Empty;
        }
    }
    public class ServiceManagerLocalization
    {
        static ManagementPack projectMP;

        public static string GetStringFromManagementPack(string resource)
        {
            try
            {
                if(projectMP == null)
                {
                    EnterpriseManagementGroup emg = ConsoleContext.GetConsoleEMG();
                    projectMP = emg.ManagementPacks.GetManagementPack(new Guid("171d60e8-a0df-e4b2-f032-a5af5c8ebe39"));
                }
                return projectMP.GetStringResource(resource).DisplayName;
            }
            catch 
            {
                return "Localization Error.";
            }
            

        }
    }
    public class ConsoleContext
    {
        private static EnterpriseManagementGroup emg;

        public static EnterpriseManagementGroup GetConsoleEMG()
        {
            try
            {
                if(emg == null)
                {
                    IServiceContainer container = (IServiceContainer)FrameworkServices.GetService(typeof(IServiceContainer));
                    IManagementGroupSession session = (IManagementGroupSession)container.GetService(typeof(IManagementGroupSession));

                    if (session != null)
                        emg = session.ManagementGroup;
                }

                if (!emg.IsConnected) emg.Reconnect();
                return emg;
            }
            catch
            {
                return null;
            }
        }
    }
    public class ProjectConnectorHelpers
    {
        public bool DeleteProjectConnector(IDataItem item)
        {
            try
            {
                //get the connector class.  Later we will get the type projection so we can delete related objects.
                //item = ConsoleContextHelper.Instance.GetInstance((Guid)item["$Id$"]);

                //get type projection of the connector (Cireson.ProjectAutomation.ConnectorRelatesToProjects.ProjectionType) (34e516cf-644f-11b7-be02-6c886ec0573b)
                item = ConsoleContextHelper.Instance.GetProjectionInstance((Guid)item["$Id$"], new Guid("34e516cf-644f-11b7-be02-6c886ec0573b"));
                //add in some sort of confirmation on delete... later.
                if (MessageBox.Show(
                    String.Format(ServiceManagerLocalization.GetStringFromManagementPack("strConfirmDelete") + " {0}", item["DisplayName"]), 
                    ServiceManagerLocalization.GetStringFromManagementPack("strDeleteConnector"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) 
                    return false;
                var emg = ConsoleContext.GetConsoleEMG();
                var emoConnector = emg.EntityObjects.GetObject<EnterpriseManagementObject>((Guid)item["$Id$"], ObjectQueryOptions.Default);
                var iddRemove = new IncrementalDiscoveryData();

                // (Cireson.ProjectAutomation.Library) (System.ProjectConfigItemRelatesToProjectConnector) (da061582-3f6c-d7b7-d17d-0a91b8a51ace)
                ManagementPackRelationship mprConnectorHasProject = emg.EntityTypes.GetRelationshipClass(new Guid("da061582-3f6c-d7b7-d17d-0a91b8a51ace"));

                //remove the related project CIs.
                foreach(EnterpriseManagementRelationshipObject<EnterpriseManagementObject> obj in 
                    emg.EntityObjects.GetRelationshipObjectsWhereSource<EnterpriseManagementObject>(emoConnector.Id, mprConnectorHasProject, DerivedClassTraversalDepth.None, TraversalDepth.OneLevel, ObjectQueryOptions.Default))
                {
                        iddRemove.Remove(obj.TargetObject);
                }    

                

                //remove workflow rule for the connector
                try
                {
                    //Cireson Project Server Automation Library Configuration (Cireson.ProjectAutomation.Library.Configuration) (19b2a173-bea9-9e50-0709-1470424916f2)
                    ManagementPack mpConnectorWorkflow = emg.ManagementPacks.GetManagementPack(new Guid("19b2a173-bea9-9e50-0709-1470424916f2"));
                    //Project Server Connector (Cireson.ProjectAutomation.Library) (Microsoft.SystemCenter.Connector.ProjectServer) (d581d2d6-b6cd-b558-7ac7-db233a7c82ec)
                    ManagementPackClass mpcConnector = emg.EntityTypes.GetClass(new Guid("d581d2d6-b6cd-b558-7ac7-db233a7c82ec"));
                    string sConnectorRuleId = string.Format("{0}.{1}", "Cireson.ProjectServer.Automation", new Guid(emoConnector[mpcConnector, "Id"].Value.ToString()).ToString("N"));
                    ManagementPackRule mprConnector = mpConnectorWorkflow.GetRule(sConnectorRuleId);
                    mprConnector.Status = ManagementPackElementStatus.PendingDelete;

                    mpConnectorWorkflow.AcceptChanges();

                }
                catch 
                { }

                iddRemove.Remove(emoConnector);
                iddRemove.Commit(emg);
                return true;
            }
            catch(Exception ex)
            {
                ConsoleContextHelper.Instance.ShowErrorDialog(ex, string.Empty, ConsoleJobExceptionSeverity.Error);
                return false;
            }
            
        }
        public WizardResult EditProjectConnector(IDataItem item)
        {
            try
            {
                WizardStory wizard = new WizardStory();
                wizard.WizardWindowTitle = ServiceManagerLocalization.GetStringFromManagementPack("strEditConnector");
                ProjectConnectorData data = new ProjectConnectorData(item);

                //setup wizard
                data.WizardMode = WizardMode.Wizard;
                wizard.WizardData = data;

                //add the wizard pages here...
                wizard.AddLast(new WizardStep(ServiceManagerLocalization.GetStringFromManagementPack("strGeneral"), typeof(GeneralWizardPage), wizard.WizardData));
                wizard.AddLast(new WizardStep(ServiceManagerLocalization.GetStringFromManagementPack("strPWAConnectionPage"), typeof(PWAWizardPage), wizard.WizardData));
                wizard.AddLast(new WizardStep(ServiceManagerLocalization.GetStringFromManagementPack("strSchedulePage"), typeof(ScheduleWizardPage), wizard.WizardData));

                PropertySheetDialog psd = new PropertySheetDialog(wizard);

                //window properties
                psd.ShowInTaskbar = true;
                psd.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                psd.Icon = BitmapFrame.Create(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.ConsoleTasks;component/Icons/Image.Cireson.16x16.ico", UriKind.RelativeOrAbsolute)).Stream);
                psd.ShowDialog();
                return data.WizardResult;
            }
            catch(Exception ex)
            {
                ConsoleContextHelper.Instance.ShowErrorDialog(ex, string.Empty, ConsoleJobExceptionSeverity.Error);
                return WizardResult.Failed;
            }
        }
        public ProjectConnectorData CreateProjectConnector()
        {
            
            try
            {
                WizardStory wizard = new WizardStory();
                //set the SCSM default connector icon here...
                wizard.WizardWindowTitle = ServiceManagerLocalization.GetStringFromManagementPack("strCreateConnector");
                ProjectConnectorData data = new ProjectConnectorData();
    
                //set the data.
                data.WizardMode = WizardMode.Wizard;
                wizard.WizardData = data;

                //pages
                //add the connector setup pages here.
                wizard.AddLast(new WizardStep(ServiceManagerLocalization.GetStringFromManagementPack("strGeneral"), typeof(GeneralWizardPage), wizard.WizardData));
                wizard.AddLast(new WizardStep(ServiceManagerLocalization.GetStringFromManagementPack("strPWAConnectionPage"), typeof(PWAWizardPage), wizard.WizardData));
                wizard.AddLast(new WizardStep(ServiceManagerLocalization.GetStringFromManagementPack("strSchedulePage"), typeof(ScheduleWizardPage), wizard.WizardData));
                wizard.AddLast(new WizardStep(ServiceManagerLocalization.GetStringFromManagementPack("strResults"), typeof(ResultsWizardPage), wizard.WizardData));

                WizardWindow wizardWindow = new WizardWindow(wizard);
                ElementHost.EnableModelessKeyboardInterop(wizardWindow);

                //window properties
                wizardWindow.ShowInTaskbar = true;
                wizardWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                wizardWindow.Icon = BitmapFrame.Create(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.ConsoleTasks;component/Icons/Image.Cireson.16x16.ico", UriKind.RelativeOrAbsolute)).Stream);
                wizardWindow.ShowDialog();
                return data;
            }
            catch(Exception ex)
            {
                ConsoleContextHelper.Instance.ShowErrorDialog(ex, string.Empty, ConsoleJobExceptionSeverity.Error);
                return null;
            }
        }

    }

}

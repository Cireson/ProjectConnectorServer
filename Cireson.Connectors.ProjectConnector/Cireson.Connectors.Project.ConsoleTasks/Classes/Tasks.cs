


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
using Microsoft.EnterpriseManagement.UI.SdkDataAccess;
using Microsoft.EnterpriseManagement.UI.Extensions;
using Microsoft.EnterpriseManagement.UI.Extensions.Shared;
using Microsoft.EnterpriseManagement.ConsoleFramework;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.UI.WpfWizardFramework;
//other references
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Forms;
using System.Windows;
using System.Windows.Media.Imaging;


namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.ConsoleTasks
{
    public class AdminSettings : ConsoleCommand
    {
        public override void ExecuteCommand(IList<NavigationModelNodeBase> nodes, NavigationModelNodeTask task, ICollection<string> parameters)
        {
            try
            {
                EnterpriseManagementGroup emg = ConsoleContext.GetConsoleEMG();
                // (Cireson.ProjectAutomation.Library) (Microsoft.SystemCenter.Connector.ProjectServer.Settings) (5a49b80c-4c34-d189-ca94-a591580f1995)
                ManagementPackClass mpcSettings = emg.EntityTypes.GetClass(new Guid("5a49b80c-4c34-d189-ca94-a591580f1995")); //get the settings class.
                EnterpriseManagementObject emoSettings = emg.EntityObjects.GetObject<EnterpriseManagementObject>(mpcSettings.Id, ObjectQueryOptions.Default); //get the settings class instance.
                //somewhere here we will have a container for our class data to feed the consoleWizard obj.
                AdminSettingsData admData = new AdminSettingsData(emoSettings);
                WizardStory consoleWizard = new WizardStory();
                consoleWizard.WizardData = admData;
                consoleWizard.AddLast(new WizardStep(ServiceManagerLocalization.GetStringFromManagementPack("strGroomingSettings"), typeof(AdminSettingsGroomingForm), admData));
                consoleWizard.AddLast(new WizardStep(ServiceManagerLocalization.GetStringFromManagementPack("strStatusSettings"), typeof(AdminSettingsStatusForm), admData));
                consoleWizard.AddLast(new WizardStep(ServiceManagerLocalization.GetStringFromManagementPack("strProjectTasksSettings"), typeof(AdminSettingsProjectTasksForm), admData));
                consoleWizard.AddLast(new WizardStep(ServiceManagerLocalization.GetStringFromManagementPack("strProjectLicensingSettings"), typeof(AdminSettingsLicensingForm), admData));
                PropertySheetDialog propertyDialog = new PropertySheetDialog(consoleWizard);
                propertyDialog.Width = 800;
                propertyDialog.Height = 700;
                propertyDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                propertyDialog.Title = ServiceManagerLocalization.GetStringFromManagementPack("strSettings");
                propertyDialog.ShowInTaskbar = true;
                propertyDialog.Icon = BitmapFrame.Create(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.ConsoleTasks;component/Icons/Image.Cireson.16x16.ico", UriKind.RelativeOrAbsolute)).Stream);
                propertyDialog.ShowDialog();
            }
            catch(Exception ex)
            {
                ConsoleContextHelper.Instance.ShowErrorDialog(ex, string.Empty, ConsoleJobExceptionSeverity.Error);
            }
        }
    }

    public class ProjectConnector : ConsoleCommand
    {
        public override void ExecuteCommand(IList<NavigationModelNodeBase> nodes, NavigationModelNodeTask task, ICollection<string> parameters)
        {
            try
            {
                EnterpriseManagementGroup emg = ConsoleContext.GetConsoleEMG();

                if (parameters.Contains("Create"))
                {
                    //do stuff
                    ProjectConnectorHelpers helper = new ProjectConnectorHelpers();
                    ProjectConnectorData data = helper.CreateProjectConnector();
                    if (data.WizardResult == WizardResult.Success)
                        this.RequestViewRefresh();
                }
                else if (parameters.Contains("Edit"))
                {
                    //do other stuff
                    ProjectConnectorHelpers helper = new ProjectConnectorHelpers();
                    WizardResult result = helper.EditProjectConnector(nodes[0]);
                    if (result == WizardResult.Success)
                        this.RequestViewRefresh();
                }
                else if (parameters.Contains("Delete"))
                {
                    //delete stuff
                    ProjectConnectorHelpers helper = new ProjectConnectorHelpers();
                    if (helper.DeleteProjectConnector(nodes[0]))
                        this.RequestViewRefresh();
                }
            }
            catch(Exception ex)
            {
                ConsoleContextHelper.Instance.ShowErrorDialog(ex, string.Empty, ConsoleJobExceptionSeverity.Error);
            }
        }
    }
}

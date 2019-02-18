


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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//service manager references
using Microsoft.EnterpriseManagement.UI.WpfWizardFramework;
using Microsoft.EnterpriseManagement.UI.WpfControls;
using Microsoft.EnterpriseManagement.UI.Extensions.Shared;
//other references
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Controls;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter;
using System.Net;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes.Licensing;


namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Forms
{
    /// <summary>
    /// Interaction logic for PWAWizardPage.xaml
    /// </summary>
    public partial class PWAWizardPage : WizardRegularPageBase
    {
        ProjectConnectorData data;
        CredentialsControl credsControl;

        

        public PWAWizardPage(WizardData wizdata)
        {
            base.Title = ServiceManagerLocalization.GetStringFromManagementPack("strPWAConnectionPage");
            this.data = wizdata as ProjectConnectorData;
            this.DataContext = data;

            if (credsControl == null)
                credsControl = new CredentialsControl(data.RunAsAccount, data.Id.ToString(), ConsoleContext.GetConsoleEMG().Name);

            if (!data.IsEditMode)
            {
                if (data.PwaUrl == null)
                    data.PwaUrl = "http://yourprojectserver/pwa";
                this.FinishButtonText = ServiceManagerLocalization.GetStringFromManagementPack("strCreateBtn");
            }


            InitializeComponent();

            credentialsPanel.Children.Add(credsControl);
            credsControl.CredentialsChanged += credsControl_CredentailsChanged;

            this.IsFinishButtonEnabled = false;
            this.IsNextButtonEnabled = false;
            this.btnTestConnection.IsEnabled = false;

        }

        void credsControl_CredentailsChanged(object sender, EventArgs e)
        {
            
            data.RunAsAccount = credsControl.EncryptedCredentials;
            if (data.IsO365)
            {
                if (string.IsNullOrEmpty(credsControl.UserName) || (credsControl.Password == null || credsControl.Password.Length == 0))
                    btnTestConnection.IsEnabled = false;
                else
                    btnTestConnection.IsEnabled = true;
            }
            else
            {
                if (string.IsNullOrEmpty(credsControl.UserName) || string.IsNullOrEmpty(credsControl.Domain) || (credsControl.Password == null || credsControl.Password.Length == 0))
                    btnTestConnection.IsEnabled = false;
                else
                    btnTestConnection.IsEnabled = true;
            }
            //creds.UserName = credsControl.UserName;
            //creds.Domain = credsControl.Domain;
            //creds.Password = PasswordHepler.ConvertSecureStringToString(credsControl.Password);
        }

        private void btnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                X1 x = new X1(ConsoleContext.GetConsoleEMG(), new Guid("5a49b80c-4c34-d189-ca94-a591580f1995"), "Project Connector");
                x.CheckLicense();

                credsControl.SetCredentials();

                Type type = Type.GetTypeFromProgID("Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Adapter.ProjectAdapter");
                object inst = Activator.CreateInstance(type);
                IProjectAdapter projAdapter = (IProjectAdapter)inst;

                if (data.IsO365)
                    projAdapter.Connect(data.PwaUrl, credsControl.UserName, PasswordHepler.ConvertSecureStringToString(credsControl.Password));
                else
                    projAdapter.Connect(data.PwaUrl, credsControl.UserName, PasswordHepler.ConvertSecureStringToString(credsControl.Password), credsControl.Domain);
                var success = projAdapter.IsConnected();

                using (new WaitCursor())
                {
                    if (success)
                    {
                        MessageBox.Show(ServiceManagerLocalization.GetStringFromManagementPack("strConnectSuccess"), ServiceManagerLocalization.GetStringFromManagementPack("strConnectorTitle"),
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        this.IsNextButtonEnabled = true;
                    }
                    else
                    {
                        MessageBox.Show(ServiceManagerLocalization.GetStringFromManagementPack("strConnectFailed"), ServiceManagerLocalization.GetStringFromManagementPack("strConnectorTitle"),
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleContextHelper.Instance.ShowErrorDialog(ex, ex.Message, ConsoleFramework.ConsoleJobExceptionSeverity.Error);
            }


        }

        private void cbUseO365_Checked(object sender, RoutedEventArgs e)
        {
            credsControl.cbDomain.IsEnabled = false;
        }

        private void cbUseO365_Unchecked(object sender, RoutedEventArgs e)
        {
            credsControl.cbDomain.IsEnabled = true;
        }

        

    }
}

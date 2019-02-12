
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
using Microsoft.EnterpriseManagement.ServiceManager.Application.Common;
using Microsoft.EnterpriseManagement.GenericForm;
using Microsoft.EnterpriseManagement.UI.DataModel;
using Microsoft.EnterpriseManagement.UI.Extensions.Shared;
//other references
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes.Licensing;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Forms
{
    /// <summary>
    /// Interaction logic for ProjectForm.xaml
    /// </summary>
    public partial class ProjectForm : UserControl
    {
        IDataItem dataInstance;

        public ProjectForm()
        {

            InitializeComponent();
            RelatedItemsPane relatedItems = new RelatedItemsPane(new ConfigItemRelatedItemsConfiguration());
            relatedItems.Name = "riRelatedItems";
            tabRelatedItems.Content = relatedItems;



            System.ComponentModel.BackgroundWorker licenseCheckThread = new System.ComponentModel.BackgroundWorker();
            licenseCheckThread.DoWork += licenseCheckThread_DoWork;
            licenseCheckThread.RunWorkerCompleted += licenseCheckThread_RunWorkerCompleted;
            licenseCheckThread.RunWorkerAsync();
           
        }

        private void licenseCheckThread_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if(e.Error != null)
            {
                lblLicenseStatus.Visibility = System.Windows.Visibility.Visible;
                grid1.IsEnabled = false;
                Microsoft.EnterpriseManagement.UI.Extensions.Shared.ConsoleContextHelper.Instance.ShowErrorDialog(e.Error, ServiceManagerLocalization.GetStringFromManagementPack("strInvalidLicense"), ConsoleFramework.ConsoleJobExceptionSeverity.Error);
            }
        }

        private void licenseCheckThread_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(1000); //sleep before we do the check.  If there is a problem, the console will need time to complete the form load before an error message is displayed.
            checkLicense();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is IDataItem)
            {
                dataInstance = (IDataItem)this.DataContext;
                lvRelatedTasks.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("SequenceId", System.ComponentModel.ListSortDirection.Ascending));
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (lvRelatedTasks.SelectedItem != null)
                FormUtilities.Instance.PopoutForm(lvRelatedTasks.SelectedItem as IDataItem);
        }

        private void lvRelatedTasks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvRelatedTasks.SelectedItem != null)
                FormUtilities.Instance.PopoutForm(lvRelatedTasks.SelectedItem as IDataItem);
        }


        void checkLicense()
        {

            X1 x = new X1(ConsoleContext.GetConsoleEMG(), new Guid("5a49b80c-4c34-d189-ca94-a591580f1995"), "Project Connector");
            x.CheckLicense();

        }

        private void tabProjectSite_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                wbProjectSite.Navigate(new Uri(dataInstance["ProjectWebUrl"].ToString()));
            }
            catch (UriFormatException ex)
            {
                Microsoft.EnterpriseManagement.UI.Extensions.Shared.ConsoleContextHelper.Instance.ShowErrorDialog(
                    ex, ServiceManagerLocalization.GetStringFromManagementPack("strInvalidProjectURI"), ConsoleFramework.ConsoleJobExceptionSeverity.Warning);
            }
        }
    }
}

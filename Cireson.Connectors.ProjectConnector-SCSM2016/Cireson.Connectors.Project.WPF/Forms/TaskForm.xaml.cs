
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
using System.Collections;
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
using Microsoft.EnterpriseManagement.GenericForm;
using Microsoft.EnterpriseManagement.UI.Extensions.Shared;
using Microsoft.EnterpriseManagement.UI.DataModel;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes.Licensing;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Forms
{
    /// <summary>
    /// Interaction logic for TaskForm.xaml
    /// </summary>
    public partial class TaskForm : UserControl
    {
        IDataItem dataInstance;
        IDataItem parentWorkItem;
        IDataItem relatedProject;
        ICollection<IDataItem> resources;

        public TaskForm()
        {
            InitializeComponent();

            System.ComponentModel.BackgroundWorker licenseCheckThread = new System.ComponentModel.BackgroundWorker();
            licenseCheckThread.DoWork += licenseCheckThread_DoWork;
            licenseCheckThread.RunWorkerCompleted += licenseCheckThread_RunWorkerCompleted;
            licenseCheckThread.RunWorkerAsync();
        }

        private void licenseCheckThread_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
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

        private void checkLicense()
        {
            X1 x = new X1(ConsoleContext.GetConsoleEMG(), new Guid("5a49b80c-4c34-d189-ca94-a591580f1995"), "Project Connector");
            x.CheckLicense();
        }

        private void TaskForm_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!FormUtilities.Instance.IsFormInTemplateMode(this))
                
            {
                if(this.DataContext is IDataItem)
                {
                    dataInstance = (IDataItem)this.DataContext;
                    if ((bool)dataInstance["$IsNew$"])
                    {
                        //assign key properties
                        dataInstance["Id"] = Guid.NewGuid().ToString();
                    }

                    if (dataInstance.HasProperty("ParentWorkItem"))
                    {
                        var parentWorkItems = dataInstance["ParentWorkItem"] as ICollection<IDataItem>;
                        if (parentWorkItems != null)
                        {
                            parentWorkItem = parentWorkItems.First();
                            hlParentWorkItem.DataContext = parentWorkItem;
                        }
                    }

                    if (!dataInstance.HasProperty("RelatedProject"))
                    {
                        //(Cireson) Project Task Activity Relates To Project (Cireson.ProjectAutomation.ProjectTaskActivityRelatesToProject.ProjectionType) (3d3c8eb9-1cb7-d2dd-bcaa-0bcc5c21a157)
                        var relatedProjectProjection = ConsoleContextHelper.Instance.GetProjectionInstance((Guid)dataInstance["$Id$"],
                                new Guid("3d3c8eb9-1cb7-d2dd-bcaa-0bcc5c21a157")); 

                        if (relatedProjectProjection.HasProperty("RelatedProject"))
                        {
                            relatedProject = relatedProjectProjection["RelatedProject"] as IDataItem;
                            hlRelatedProject.DataContext = relatedProject;
                        }
                    }
                    else
                    {
                        relatedProject = dataInstance["RelatedProject"] as IDataItem;
                        hlRelatedProject.DataContext = relatedProject;
                    }

                    if(!dataInstance.HasProperty("RelatedResource"))
                    {
                        //(Cireson) Project Task Activity Has Resource (Cireson.ProjectAutomation.ProjectTaskActivityHasResource.ProjectionType) (43b76917-318f-9d41-db49-50a42e9fb7e2)
                        var TaskHasResourceProjection = ConsoleContextHelper.Instance.GetProjectionInstance((Guid)dataInstance["$Id$"],
                            new Guid("43b76917-318f-9d41-db49-50a42e9fb7e2"));

                        if(TaskHasResourceProjection.HasProperty("RelatedResource"))
                        {
                            resources = TaskHasResourceProjection["RelatedResource"] as ICollection<IDataItem>;
                            lvResources.DataContext = resources;
                            lvResources.ItemsSource = resources;
                        }
                    }

                }

               
            }
        }

        private void hlParentWorkItem_Click(object sender, RoutedEventArgs e)
        {
            if (parentWorkItem != null)
                FormUtilities.Instance.OpenParentWI(dataInstance);
        }

        private void hlRelatedProject_Click(object sender, RoutedEventArgs e)
        {
            if (relatedProject != null)
                FormUtilities.Instance.PopoutForm(relatedProject);
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (lvResources.SelectedItem != null)
                FormUtilities.Instance.PopoutForm(lvResources.SelectedItem as IDataItem);
        }

        private void lvResources_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvResources.SelectedItem != null)
                FormUtilities.Instance.PopoutForm(lvResources.SelectedItem as IDataItem);
        }


    }
}

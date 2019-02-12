
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//serivce manager references
using Microsoft.EnterpriseManagement.UI.WpfWizardFramework;
using Microsoft.EnterpriseManagement.UI.WpfControls;
using Microsoft.EnterpriseManagement.Configuration;
//other references
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Forms
{
    /// <summary>
    /// Interaction logic for AdminSettingsProjectTasksForm.xaml
    /// </summary>
    public partial class AdminSettingsProjectTasksForm : WizardRegularPageBase
    {
        AdminSettingsData data;

        public AdminSettingsProjectTasksForm(WizardData _data)
        {
            
            InitializeComponent();
            this.data = _data as AdminSettingsData;
            this.DataContext = this.data;

            //add interval units
            cbFrequencyUnit.Items.Add(ServiceManagerLocalization.GetStringFromManagementPack("strHours"));
            cbFrequencyUnit.Items.Add(ServiceManagerLocalization.GetStringFromManagementPack("strMinutes"));
            //set interval unit if the data has it.
            cbFrequencyUnit.Text = data.TaskEvalFrequencyUnit;
            
        }

        private void cbFrequencyUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            data.TaskEvalFrequencyUnit = cbFrequencyUnit.SelectedValue.ToString();
        }

    }

}

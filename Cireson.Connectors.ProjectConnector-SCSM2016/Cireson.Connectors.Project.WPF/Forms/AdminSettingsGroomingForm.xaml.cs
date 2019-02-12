
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
//other references
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Forms
{
    /// <summary>
    /// Interaction logic for AdminSettingsGroomingForm.xaml
    /// </summary>
    public partial class AdminSettingsGroomingForm : WizardRegularPageBase
    {
        AdminSettingsData settings;

        public AdminSettingsGroomingForm(WizardData data)
        {
            settings = data as AdminSettingsData;
            this.DataContext = settings;
            InitializeComponent();
            numericDays.Value = Convert.ToDecimal(settings.RetentionDays);
            spDataRetentionTime.IsEnabled = settings.IsRetentionEnabled;
        }

        private void cbRetentionEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if(spDataRetentionTime != null)
                spDataRetentionTime.IsEnabled = true;
        }

        private void cbRetentionEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            spDataRetentionTime.IsEnabled = false;
        }

        public override void SaveState(SavePageEventArgs e)
        {
            settings.RetentionDays = Convert.ToInt32(numericDays.Value);
        }

    }
}

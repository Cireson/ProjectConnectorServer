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

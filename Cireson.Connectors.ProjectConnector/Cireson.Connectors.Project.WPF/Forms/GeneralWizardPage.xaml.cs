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
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes.Licensing;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Forms
{
    /// <summary>
    /// Interaction logic for GeneralWizardPage.xaml
    /// </summary>
    public partial class GeneralWizardPage : WizardRegularPageBase
    {
        ProjectConnectorData data;

        public GeneralWizardPage(WizardData wizardData)
        {
            base.Title = ServiceManagerLocalization.GetStringFromManagementPack("strGeneral");
            this.data = wizardData as ProjectConnectorData;
            this.DataContext = data;
            if (!data.IsEditMode)
                this.FinishButtonText = ServiceManagerLocalization.GetStringFromManagementPack("strCreateBtn");
            this.IsNextButtonEnabled = false;
            InitializeComponent();
        }

        private void WizardRegularPageBase_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Focus();
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
                this.IsNextButtonEnabled = false;
            else
                this.IsNextButtonEnabled = true;
        }


    }
}

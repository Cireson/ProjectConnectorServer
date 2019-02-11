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
//other references
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Forms
{
    /// <summary>
    /// Interaction logic for ResultsWizardPage.xaml
    /// </summary>
    public partial class ResultsWizardPage : WizardResultPageBase
    {
        public ResultsWizardPage(WizardData wizardData)
        {
            base.Title = ServiceManagerLocalization.GetStringFromManagementPack("strResults");
            if (wizardData is ProjectConnectorData)
                this.DataContext = wizardData as ProjectConnectorData;
            //else if (wizardData is ProjectConfigurationWizardData)
            //    this.DataContext = wizardData as ProjectConfigurationWizardData;
            this.FinishButtonText = ServiceManagerLocalization.GetStringFromManagementPack("strClose");
            this.IsFinishButtonEnabled = true;
            InitializeComponent();
        }
    }
}

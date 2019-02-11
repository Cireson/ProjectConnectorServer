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
//service Manager references
using Microsoft.EnterpriseManagement.UI.WpfWizardFramework;
//other references
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Forms
{
    /// <summary>
    /// Interaction logic for AdminSettingsLicensing.xaml
    /// </summary>
    public partial class AdminSettingsLicensingForm : WizardRegularPageBase
    {
        AdminSettingsData settings;

        public AdminSettingsLicensingForm(WizardData data)
        {
            settings = data as AdminSettingsData;
            this.DataContext = settings;
            InitializeComponent();
        }
    }
}

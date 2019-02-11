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
    /// Interaction logic for ScheduleWizardPage.xaml
    /// </summary>
    public partial class ScheduleWizardPage : WizardRegularPageBase
    {
        ProjectConnectorData data;

        public ScheduleWizardPage(WizardData wizardData)
        {
            this.Title = ServiceManagerLocalization.GetStringFromManagementPack("strSchedulePage");
            this.data = wizardData as ProjectConnectorData;
            this.DataContext = this.data;

            if (!data.IsEditMode)
                this.FinishButtonText = ServiceManagerLocalization.GetStringFromManagementPack("strCreateBtn");

            this.IsNextButtonEnabled = false;
            this.IsFinishButtonEnabled = false;
            InitializeComponent();

            //add interval units
            cbFrequencyUnit.Items.Add(ServiceManagerLocalization.GetStringFromManagementPack("strHours"));
            cbFrequencyUnit.Items.Add(ServiceManagerLocalization.GetStringFromManagementPack("strMinutes"));
            //set interval unit if the data has it.
            cbFrequencyUnit.Text = data.FrequencyUnit;

        }

        private void rbDailySchedule_Checked(object sender, RoutedEventArgs e)
        {
            if (spFrequencySchedule != null)
            {
                spFrequencySchedule.IsEnabled = false;
                spDailySchedule.IsEnabled = true;
                rbFrequencySchedule.IsChecked = false;
                validateSchedule();
            }
        }

        private void rbFrequencySchedule_Checked(object sender, RoutedEventArgs e)
        {
            if (spFrequencySchedule != null)
            {
                spDailySchedule.IsEnabled = false;
                spFrequencySchedule.IsEnabled = true;
                rbDailySchedule.IsChecked = false;
                validateSchedule();
            }
        }

        void validateSchedule()
        {
            if(data.IsDailySchedule)
            {
                //update the bindings
                if(txtTime != null)
                    txtTime.GetBindingExpression(TextBox.TextProperty).UpdateSource();

                if ((data.IsSunday || data.IsMonday || data.IsTuesday || data.IsWednesday || data.IsThursday || data.IsFriday || data.IsSaturday) &
                    !string.IsNullOrEmpty(data.ScheduledTime))
                {
                    this.IsFinishButtonEnabled = true;
                }
                else
                    this.IsFinishButtonEnabled = false;
            }
            else
            {
                if(txtFrequency != null)
                    txtFrequency.GetBindingExpression(TextBox.TextProperty).UpdateSource();

                if (cbFrequencyUnit != null)
                    if (data.FrequencyInterval == 0 || string.IsNullOrEmpty(data.FrequencyUnit))
                        this.IsFinishButtonEnabled = false;
                    else
                        this.IsFinishButtonEnabled = true;
            }
        }

        private void cbFrequencyUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            data.FrequencyUnit = cbFrequencyUnit.SelectedValue.ToString();
            validateSchedule();
        }

        private void txtFrequency_TextChanged(object sender, TextChangedEventArgs e)
        {
            validateSchedule();
        }

        private void txtTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            validateSchedule();
        }


        private void cbDays_Validate(object sender, RoutedEventArgs e)
        {
            validateSchedule();
        }

    }
}

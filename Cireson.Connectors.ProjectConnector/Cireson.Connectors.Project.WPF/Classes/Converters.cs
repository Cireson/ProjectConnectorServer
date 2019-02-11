using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//other references
using System.Windows.Data;
using System.Globalization;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes
{
    //this is cool Travis. :) Pull the localized info from an MP to display on a WPF form.
    public class StringConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type target, object parameter, CultureInfo culture)
        {
            try
            {
                var strId = (string)parameter;
                return ServiceManagerLocalization.GetStringFromManagementPack(strId);
            }
            catch
            {
                return value as string;
            }
        }

        object IValueConverter.ConvertBack(object value, Type target, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

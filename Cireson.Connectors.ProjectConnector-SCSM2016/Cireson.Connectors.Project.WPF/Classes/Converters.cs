
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

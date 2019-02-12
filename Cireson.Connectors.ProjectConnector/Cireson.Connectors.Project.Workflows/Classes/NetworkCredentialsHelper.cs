
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
using System.Threading.Tasks;
//other references
using System.Xml;
using System.Xml.XPath;
using System.Net;
using System.Security;
//project adapter references
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.Licensing
{
    public static class NetworkCredentialsHelper
    {
        public static NetworkCredential GetProjectCredentials(string runAs, string guid, string name)
        {
            try
            {
                if (!string.IsNullOrEmpty(runAs))
                {
                    var xml = PasswordHepler.GetFullString(runAs, guid, name);

                    if (!string.IsNullOrEmpty(xml))
                    {
                        //read the xml
                        if (!xml.EndsWith("/>"))
                            xml = xml.Substring(0, xml.IndexOf("/>") + 2);


                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(new System.IO.StringReader(xml));
                        XPathNavigator xmlNav = xmlDoc.CreateNavigator();
                        xmlNav.MoveToFirstChild();
                        xmlNav.MoveToFirstAttribute();

                        NetworkCredential creds = new NetworkCredential();

                        do
                        {
                            if (xmlNav.Name == "Username")
                            {
                                creds.UserName = xmlNav.Value;
                            }
                            if (xmlNav.Name == "Domain")
                            {
                                creds.Domain = xmlNav.Value;
                            }
                            if (xmlNav.Name == "Password")
                            {
                                SecureString password = new SecureString();
                                foreach (char c in xmlNav.Value)
                                    password.AppendChar(c);
                                creds.SecurePassword = password;

                            }

                        } while (xmlNav.MoveToNextAttribute());

                        return creds;

                    }
                    return null;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

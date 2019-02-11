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

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
//other references
using System.Net;
using System.DirectoryServices.ActiveDirectory;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes;
using System.Xml;
using System.Xml.XPath;
using System.Security;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Controls
{
    /// <summary>
    /// Interaction logic for Credentials.xaml
    /// </summary>
    public partial class CredentialsControl : UserControl
    {
        string encryptedData;
        string Id;
        string emgName;

        public string UserName { get; set; }
        public string Domain { get; set; }
        public SecureString Password { get; set; }
        public string EncryptedCredentials { get; set; }
        

        public event EventHandler CredentialsChanged;


        public CredentialsControl(string runas, string connectorId, string emgName)
        {
            encryptedData = runas;
            Id = connectorId;
            this.emgName = emgName;
            this.DataContext = this;
            InitializeComponent();
            loadDomains();
            getCredentails();
        }

        public void SetCredentials()
        {
            EncryptedCredentials = PasswordHepler.SetFullString(
                "<Credentails Username=\"" + UserName + "\" Password=\"" + PasswordHepler.ConvertSecureStringToString(Password) + "\" Domain=\"" + Domain + "\" />", Id, emgName);
            if (this.CredentialsChanged != null)
                CredentialsChanged(this, new EventArgs());
        }

        void getCredentails()
        {
            try
            {
                if(!string.IsNullOrEmpty(encryptedData))
                {
                    var xml = PasswordHepler.GetFullString(encryptedData, Id, emgName);

                    if(!string.IsNullOrEmpty(xml))
                    {
                        //read the xml
                        if (!xml.EndsWith("/>"))
                            xml = xml.Substring(0, xml.IndexOf("/>") + 2);

                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(new System.IO.StringReader(xml));
                        XPathNavigator xmlNav = xmlDoc.CreateNavigator();
                        xmlNav.MoveToFirstChild();
                        xmlNav.MoveToFirstAttribute();

                        do
                        {
                            if (xmlNav.Name == "Username")
                            {
                                UserName = xmlNav.Value;
                                txtUsername.Text = UserName;
                            }
                            if (xmlNav.Name == "Domain")
                            {
                                Domain = xmlNav.Value;
                                cbDomain.Text = Domain;
                            }
                            if (xmlNav.Name == "Password")
                            {
                                if (Password == null)
                                    Password = new SecureString();
                                foreach (char c in xmlNav.Value)
                                    Password.AppendChar(c);

                                pbPassword.Password = xmlNav.Value;
                            }


                                    
                        } while (xmlNav.MoveToNextAttribute());
                    }
                }
            }
            catch { }
        }
        
        void loadDomains()
        {
            using (var forest = Forest.GetCurrentForest())
            {
                foreach (Domain domain in forest.Domains)
                {
                    cbDomain.Items.Add(domain.Name);
                    domain.Dispose();
                }
            }
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            //cannot use binding.  This event fires before binding occurs. :(
            this.UserName = txtUsername.Text;
            if (this.CredentialsChanged != null)
                CredentialsChanged(this, new EventArgs());
        }


        private void cbDomain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Domain = cbDomain.SelectedValue.ToString();
            if (this.CredentialsChanged != null)
                CredentialsChanged(this, new EventArgs());
        }

        private void pbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //SetCredentials();
            this.Password = pbPassword.SecurePassword;
            if (this.CredentialsChanged != null)
                CredentialsChanged(this, new EventArgs());
        }


    }
}

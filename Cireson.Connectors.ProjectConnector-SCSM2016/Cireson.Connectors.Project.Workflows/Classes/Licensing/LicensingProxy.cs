
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
using System.IO;
using System.Linq;
using System.Reflection;
using LicenseManagement.Client;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.Classes.Licensing
{
    internal static class LicensingProxy
    {

        public static ILicenseValidatorProxy LoadLicensingDomain(EnterpriseManagementObject appSettings)
        {
            var domainSetup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                DisallowApplicationBaseProbing = false,
                DisallowBindingRedirects = true,
                DisallowCodeDownload = true,
                DisallowPublisherPolicy = true
            };

            var domain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), null, domainSetup);

            

            using (var stream = GetLicenseClientResource(appSettings))
            {

                var proxy =
                    domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName,
                        typeof(LicenseValidatorProxy).FullName, false, BindingFlags.Default, null, new[] { stream }, null, null, null) as
                        ILicenseValidatorProxy;

                if (proxy == null)
                    throw new Exception("Cannot load proxy.");

                return proxy;
            }

            
        }
        private static MemoryStream GetLicenseClientResource(EnterpriseManagementObject appSettings)
        {
            var mp = appSettings.GetLeastDerivedNonAbstractClass().GetManagementPack();

            var assemblyResource =
                mp.GetResources<ManagementPackResource>()
                    .FirstOrDefault(r => r.FileName == "LicenseManagement.Client.dll");

            return appSettings.ManagementGroup.Resources.GetResourceData(assemblyResource) as MemoryStream;
        }
    }

    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    internal interface ILicenseValidatorProxy
    {
        void LoadLicensingClient(MemoryStream assembly);
        bool ValidateLicense(string productKey, string applicationName, string licenseKey, string version);

        bool ValidateLicense(string productKey, string applicationName, string licenseKey, string version,
            out DateTime? expirationDate);
    }

    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    internal class LicenseValidatorProxy : MarshalByRefObject, ILicenseValidatorProxy
    {
        public delegate object ObjectActivator(params object[] args);

        [NonSerialized]
        private ILicenseValidator _validator;

        public LicenseValidatorProxy(MemoryStream assembly)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.Load(assembly.ToArray());

        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name.Split(',')[0]);
            if (name.Name == "LicenseManagement.Client")
                return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            return null;
        }

        public void LoadLicensingClient(MemoryStream assembly)
        {
            AppDomain.CurrentDomain.Load(assembly.ToArray());
        }

        public bool ValidateLicense(string productKey,  string applicationName, string licenseKey, string version)
        {
            _validator = new LicenseValidator();

            try
            {
                return _validator.ValidateLicense(productKey, applicationName, licenseKey, version) != null;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        public bool ValidateLicense(string productKey, string applicationName, string licenseKey, string version, out DateTime? expirationDate)
        {
            _validator = new LicenseValidator();

            try
            {
                var license = _validator.ValidateLicense(productKey, applicationName, licenseKey, version);
                if (license == null)
                {
                    expirationDate = null;
                    return false;
                }
                expirationDate = license.LicenseKeyExpirationDate;
                return  true;
            }
            catch (Exception)
            {
                expirationDate = null;
                return false;
            }

        }

    }

}

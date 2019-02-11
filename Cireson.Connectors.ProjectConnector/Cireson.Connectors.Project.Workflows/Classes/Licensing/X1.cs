using System;
using Microsoft.EnterpriseManagement.Common;

namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.Workflows.Classes.Licensing
{
    public interface IX1
    {
        string CheckLicense();
    }

    public class X1 : IX1
    {
        private readonly EnterpriseManagementGroup _emg;
        private readonly EnterpriseManagementObject _emoSettings;
        private readonly string _productName;

        public X1(EnterpriseManagementGroup emg, Guid settingsGuid, string productName)
        {
            _emg = emg;
            _productName = productName;

            _emoSettings = _emg.EntityObjects.GetObject<EnterpriseManagementObject>(settingsGuid, ObjectQueryOptions.Default);

        }

        public string CheckLicense()
        {
            try
            {
                //check if license MP exists.
                _emg.ManagementPacks.GetManagementPack(new Guid("d3818ae4-e8e0-5f64-9411-d142cdd84dc9"));

                //var license = validator.ValidateLicense(getProductKey(), _productName, getLicenseKey());


                //var fakelicense = new Mock<ILicenseValidator>();
                //fakelicense.Setup(f => f.ValidateLicense(getProductKey(), _productName, getLicenseKey())).Returns(() => new License("TESTKEY", DateTime.Now, _productName));
                //validator = fakelicense.Object;

                var version = _emoSettings.GetLeastDerivedNonAbstractClass().GetManagementPack().Version.ToString();
                var productKey = GetProductKey();
                var licenseKey = GetLicenseKey();

                var proxy = LicensingProxy.LoadLicensingDomain(_emoSettings);

                DateTime? expirationDate = null;
                if(proxy.ValidateLicense(productKey, _productName, licenseKey, version, out expirationDate))
                    return expirationDate.ToString();

                throw new LicenseNotFoundException("Invalid license key or no license found.");


            }
            catch (ObjectNotFoundException ex)
            {
                throw new LicenseNotFoundException("Cireson Licensing Management Pack is not installed on the management group. " + ex.Message, ex);
            }
            catch (LicenseNotFoundException ex)
            {
                throw new LicenseNotFoundException("An error occured while retrieving license for " + _productName + ". " + ex.Message, ex);
            }

        }

        private string GetLicenseKey()
        {
            //ManagementPackClass mpcSettings = _emg.EntityTypes.GetClass(new Guid("5a49b80c-4c34-d189-ca94-a591580f1995")); //Project Server Settings Class.   
            return _emoSettings[null, "Key"].Value as string;
        }

        private string GetProductKey()
        {
            EnterpriseManagementObject emoLicenceSettings = _emg.EntityObjects.GetObject<EnterpriseManagementObject>(new Guid("a0dc046a-66eb-a926-23b7-9e060fdf6fe9"), ObjectQueryOptions.Default);
            return emoLicenceSettings[null, "ProductKey"].Value as string;
        }

        void SaveLicenseKey(string key)
        {
            _emoSettings[null, "Key"].Value = key;
            _emoSettings.Overwrite();
        }
    }

    internal class LicenseNotFoundException : Exception
    {
        public LicenseNotFoundException() { }
        public LicenseNotFoundException(string message) : base(message) { }
        public LicenseNotFoundException(string message, Exception inner) : base(message, inner) { }

    }
}

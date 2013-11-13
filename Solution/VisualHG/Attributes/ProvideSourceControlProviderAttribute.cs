using System;
using System.Globalization;
using Microsoft.VisualStudio.Shell;

namespace VisualHg
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ProvideSourceControlProviderAttribute : RegistrationAttribute
	{
        public Guid ProviderGuid
        {
            get { return Guids.guidSccProvider; }
        }

        public Guid PackageGuid
        {
            get { return Guids.guidSccProviderPkg; }
        }

        public Guid ServiceGuid
        {
            get { return Guids.guidSccProviderService; }
        }

        public string ProviderName { get; private set; }
        
        public string PackageName { get; private set; }


        public ProvideSourceControlProviderAttribute(string providerName, string packageName)
		{
            ProviderName = providerName;
            PackageName = packageName;
    	}


        public override void Register(RegistrationContext context)
		{
            using (var key = context.CreateKey(GetKeyName()))
            {
                key.SetValue("", ProviderName);
                key.SetValue("Service", ServiceGuid.ToString("B"));

                using (var subKey = key.CreateSubkey("Name"))
                {
                    subKey.SetValue("", PackageName);
                    subKey.SetValue("Package", PackageGuid.ToString("B"));

                    subKey.Close();
                }

                key.Close();
            }
		}

        public override void Unregister(RegistrationContext context)
		{
            context.RemoveKey(GetKeyName());
		}

        private string GetKeyName()
        {
            return String.Format(@"SourceControlProviders\{0:B}", ProviderGuid);
        }
	}
}

using System;
using Microsoft.VisualStudio.Shell;

namespace VisualHg
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ProvideSourceControlProviderAttribute : RegistrationAttribute
    {
        public string PackageName { get; }

        public Guid ProviderGuid { get; }

        public Guid ServiceGuid { get; }

        public Guid PackageGuid { get; }


        public ProvideSourceControlProviderAttribute(string packageName, string providerGuid, string serviceGuid,
            string packageGuid)
        {
            PackageName = packageName;
            ProviderGuid = new Guid(providerGuid);
            ServiceGuid = new Guid(serviceGuid);
            PackageGuid = new Guid(packageGuid);
        }


        public override void Register(RegistrationContext context)
        {
            using (var key = context.CreateKey(GetKeyName()))
            {
                key.SetValue("", PackageName);
                key.SetValue("Service", ServiceGuid.ToString("B"));

                using (var subKey = key.CreateSubkey("Name"))
                {
                    subKey.SetValue("", PackageName);
                    subKey.SetValue("Package", PackageGuid.ToString("B"));
                }
            }
        }

        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey(GetKeyName());
        }

        private string GetKeyName()
        {
            return string.Format(@"SourceControlProviders\{0:B}", ProviderGuid);
        }
    }
}
using System;
using Microsoft.VisualStudio.Shell;

namespace VisualHg
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ProvideSolutionPersistenceAttribute : RegistrationAttribute
    {
        public string PackageName { get; private set; }

        public Guid PackageGuid { get; private set; }


        public ProvideSolutionPersistenceAttribute(string packageName, string packageGuid)
        {
            PackageName = packageName;
            PackageGuid = new Guid(packageGuid);
        }

        public override void Register(RegistrationContext context)
        {
            using (var key = context.CreateKey(GetKeyName()))
            {
                key.SetValue("", PackageGuid.ToString("B"));
            }
        }

        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey(GetKeyName());
        }

        private string GetKeyName()
        {
            return String.Format(@"{0}\{1}", "SolutionPersistence", PackageName);
        }
    }
}

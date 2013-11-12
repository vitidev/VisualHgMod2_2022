using System;
using Microsoft.VisualStudio.Shell;

namespace VisualHg
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ProvideSolutionPersistenceAttribute : RegistrationAttribute
    {
        private Guid PackageGuid
        {
            get { return Guids.guidSccProviderPkg; }
        }

        public string PackageName { get; private set; }


        public ProvideSolutionPersistenceAttribute(string packageName)
        {
            PackageName = packageName;
        }

        public override void Register(RegistrationContext context)
        {
            using (var key = context.CreateKey(GetKeyName()))
            {
                key.SetValue("", PackageGuid.ToString("B"));
                key.Close();
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

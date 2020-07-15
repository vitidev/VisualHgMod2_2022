using System;
using Microsoft.VisualStudio.Shell;

namespace VisualHg
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ProvideOptionsPageVisibilityAttribute : RegistrationAttribute
    {
        public string CategoryName { get; }

        public string PageName { get; }

        public Guid CommandUIGuid { get; }


        public ProvideOptionsPageVisibilityAttribute(string categoryName, string pageName, string commandUIGuid)
        {
            CategoryName = categoryName;
            PageName = pageName;
            CommandUIGuid = new Guid(commandUIGuid);
        }


        public override void Register(RegistrationContext context)
        {
            using (var key = context.CreateKey(GetKeyName()))
            {
                key.SetValue(CommandUIGuid.ToString("B"), 1);
            }
        }

        public override void Unregister(RegistrationContext context)
        {
            context.RemoveValue(GetKeyName(), CommandUIGuid.ToString("B"));
        }

        private string GetKeyName()
        {
            return string.Format(@"ToolsOptionsPages\{0}\{1}\VisibilityCmdUIContexts", CategoryName, PageName);
        }
    }
}
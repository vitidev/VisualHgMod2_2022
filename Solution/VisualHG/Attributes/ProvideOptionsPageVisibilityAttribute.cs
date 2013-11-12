using System;
using Microsoft.VisualStudio.Shell;

namespace VisualHg
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ProvideOptionsPageVisibilityAttribute : RegistrationAttribute
	{
        public string CategoryName { get; private set; }

        public string PageName { get; private set; }

        public Guid CommandUIGuid { get; private set; }
        

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
                key.Close();
            }
		}

        public override void Unregister(RegistrationContext context)
        {
            context.RemoveValue(GetKeyName(), CommandUIGuid.ToString("B"));
        }

        private string GetKeyName()
        {
            return String.Format(@"ToolsOptionsPages\{0}\{1}\VisibilityCmdUIContexts", CategoryName, PageName);
        }
    }
}

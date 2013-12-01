using System;
using System.Reflection;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace VisualHg.ViewModel
{
    public static class EnvironmentColors
    {
        private static readonly Type environmentColors = LoadEnvironmentColorsType();

        public static readonly object HeaderBorderColorKey = GetKey("InactiveBorderColorKey", SystemColors.ControlColorKey);
        public static readonly object HeaderColorKey = GetKey("ToolWindowBackgroundColorKey", SystemColors.ControlLightColorKey);
        public static readonly object HeaderMouseDownColorKey = GetKey("CommandBarMouseDownBackgroundBeginColorKey", SystemColors.HighlightColorKey);
        public static readonly object HeaderMouseDownTextColorKey = GetKey("CommandBarTextMouseDownColorKey", SystemColors.HighlightTextColorKey);
        public static readonly object HeaderMouseOverColorKey = GetKey("CommandBarMouseOverBackgroundBeginColorKey", SystemColors.ControlLightLightColorKey);
        public static readonly object HeaderTextColorKey = GetKey("ToolWindowTextColorKey", SystemColors.WindowTextColorKey);
        public static readonly object HighlightColorKey = GetKey("SystemHighlightColorKey", SystemColors.HighlightColorKey);
        public static readonly object HighlightTextColorKey = GetKey("SystemHighlightTextColorKey", SystemColors.HighlightTextColorKey);
        public static readonly object InactiveSelectionHighlightColorKey = GetKey("InactiveBorderColorKey", SystemColors.HighlightColorKey);
        public static readonly object InactiveSelectionHighlightTextColorKey = GetKey("ToolWindowTextColorKey", SystemColors.HighlightTextColorKey);
        public static readonly object MenuBorderColorKey = GetKey("DropDownPopupBorderColorKey", SystemColors.ActiveBorderColorKey);
        public static readonly object MenuColorKey = GetKey("DropDownPopupBackgroundBeginColorKey", SystemColors.MenuColorKey);
        public static readonly object MenuHighlightColorKey = GetKey("CommandBarMenuItemMouseOverColorKey", SystemColors.HighlightColorKey);
        public static readonly object MenuHighlightTextColorKey = GetKey("CommandBarMenuItemMouseOverTextColorKey", SystemColors.HighlightTextColorKey);
        public static readonly object WindowColorKey = GetKey("ToolWindowBackgroundColorKey", SystemColors.WindowColorKey);
        public static readonly object WindowTextColorKey = GetKey("ToolWindowTextColorKey", SystemColors.WindowTextColorKey);


        private static Type LoadEnvironmentColorsType()
        {
            if (VisualHgPackage.VsVersion > 10)
            {
                try
                {
                    var assembly = Assembly.Load("Microsoft.VisualStudio.Shell.11.0, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");
                    return assembly.GetType("Microsoft.VisualStudio.PlatformUI.EnvironmentColors");
                }
                catch { }
            }

            return null;
        }

        private static object GetKey(string name, ResourceKey resourceKey)
        {
            if (environmentColors == null)
            {
                return resourceKey;
            }

            return GetValue(environmentColors, name);
        }

        private static object GetValue(Type type, string name)
        {
            return type.GetProperty(name).GetValue(null, BindingFlags.Static, null, null, null);
        }
    }
}
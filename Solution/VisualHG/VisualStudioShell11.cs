using System;
using System.Reflection;

namespace VisualHg
{
    public static class VisualStudioShell11
    {
        private static readonly Assembly Shell =
            Load(
                "Microsoft.VisualStudio.Shell.15.0, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");

        private static readonly Assembly Interop =
            Load(
                "Microsoft.VisualStudio.Shell.Interop.15.0, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");

        private static Assembly Load(string assemblyString)
        {
            if (VisualHgPackage.VsVersion == 10)
                return null;

            try
            {
                return Assembly.Load(assemblyString);
            }
            catch
            {
                return null;
            }
        }

        public static Type GetType(string name)
        {
            if (Shell == null)
                return null;

            try
            {
                return Shell.GetType(name) ?? Interop.GetType(name);
            }
            catch
            {
                return null;
            }
        }
    }
}
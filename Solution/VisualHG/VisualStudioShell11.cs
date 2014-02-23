using System;
using System.Reflection;

namespace VisualHg
{
    public static class VisualStudioShell11
    {
        private static readonly Assembly shell = Load("Microsoft.VisualStudio.Shell.11.0, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");
        private static readonly Assembly interop = Load("Microsoft.VisualStudio.Shell.Interop.11.0, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");

        private static Assembly Load(string assemblyString)
        {
            if (VisualHgPackage.VsVersion == 10)
            {
                return null;
            }

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
            if (shell == null)
            {
                return null;
            }

            try
            {
                return shell.GetType(name) ?? interop.GetType(name);
            }
            catch
            {
                return null;
            }
        }
    }
}
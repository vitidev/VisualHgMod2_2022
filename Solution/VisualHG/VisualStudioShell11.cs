using System;
using System.Reflection;

namespace VisualHg.ViewModel
{
        public static class VisualStudioShell11
        {
            private static readonly Assembly assembly = Load();

            private static Assembly Load()
            {
                if (VisualHgPackage.VsVersion == 10)
                {
                    return null;
                }

                try
                {
                    return Assembly.Load("Microsoft.VisualStudio.Shell.11.0, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");
                }
                catch
                {
                    return null;
                }
            }

            public static Type GetType(string name)
            {
                if (assembly == null)
                {
                    return null;
                }

                try
                {
                    return assembly.GetType(name);
                }
                catch
                {
                    return null;
                }
            }
        }
}
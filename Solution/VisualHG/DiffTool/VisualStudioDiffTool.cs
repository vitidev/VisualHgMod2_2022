using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualHg
{
    public class VsDiffTool : DiffTool
    {
        private static readonly Type serviceType;
        private static readonly MethodInfo diffMethod;

        public static bool IsAvailable
        {
            get { return serviceType != null && diffMethod != null; }
        }

        static VsDiffTool()
        {
            var interfaceType = VisualStudioShell11.GetType("Microsoft.VisualStudio.Shell.Interop.IVsDifferenceService");

            if (interfaceType != null)
            {
                diffMethod = interfaceType.GetMethod("OpenComparisonWindow2", new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(uint) });
            }

            serviceType = VisualStudioShell11.GetType("Microsoft.VisualStudio.Shell.Interop.SVsDifferenceService");
        }


        public override void Start(string fileA, string fileB, string nameA, string nameB, string workingDirectory)
        {
            OpenComparisonWindow2(fileA, fileB, "Diff - {1}", null, nameA, nameB, String.Format("{0} => {1}", nameA, nameB), null, 0);
        }

        private static IVsWindowFrame OpenComparisonWindow2(string leftFileMoniker, string rightFileMoniker, string caption, string Tooltip, string leftLabel, string rightLabel, string inlineLabel, string roles, uint grfDiffOptions)
        {
            object diffService = GetDiffService();
            object returnValue = null;

            if (diffService != null && diffMethod != null)
            {
                returnValue = diffMethod.Invoke(diffService, new object[] { leftFileMoniker, rightFileMoniker, caption, Tooltip, leftLabel, rightLabel, inlineLabel, roles, grfDiffOptions });
            }

            return returnValue as IVsWindowFrame;
        }

        private static object GetDiffService()
        {
            if (serviceType == null)
            {
                return null;
            }

            return Package.GetGlobalService(serviceType);
        }
    }
}

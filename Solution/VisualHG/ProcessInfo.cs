using System.Diagnostics;
using System.Linq;
using System.Management;

namespace VisualHg
{
    public static class ProcessInfo
    {
        public static Process[] GetChildProcesses(Process process)
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ParentProcessId=" + process.Id);

            return searcher.Get()
                .Cast<ManagementObject>()
                .Select(x => Process.GetProcessById((int)(uint)x["ProcessId"]))
                .ToArray();
        }
    }
}
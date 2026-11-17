using System.Diagnostics;
using System.Globalization;
using System.Management;

namespace Discord_Bot.Tools.Extensions;

public static class ProcessExtension
{
    //The caller owns the returned processes and has to dispose them
    public static IList<Process> GetChildProcesses(this Process process)
    {
        using (ManagementObjectSearcher searcher = new($"Select * From Win32_Process Where ParentProcessID={process.Id}"))
        {
            using (ManagementObjectCollection children = searcher.Get())
            {
                return children
                    .Cast<ManagementObject>()
                    .Select(mo =>
                    {
                        try
                        {
                            return Process.GetProcessById(Convert.ToInt32(mo["ProcessID"], CultureInfo.InvariantCulture));
                        }
                        catch (Exception) { }
                        return null;
                    })
                    .OfType<Process>()
                    .ToList();
            }
        }
    }

    //Called every second by the diagnostics, so the processes are disposed right after counting
    public static int GetChildProcessCount(this Process process)
    {
        IList<Process> children = process.GetChildProcesses();
        foreach (Process child in children)
        {
            child.Dispose();
        }

        return children.Count;
    }
}

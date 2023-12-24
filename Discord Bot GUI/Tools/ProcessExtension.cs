using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Discord_Bot.Tools
{
    public static class ProcessExtension
    {
        public static IList<Process> GetChildProcesses(this Process process)
        {
            return new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={process.Id}")
                        .Get()
                        .Cast<ManagementObject>()
                        .Select(mo =>
                        {
                            try
                            {
                                return Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]));
                            }
                            catch (Exception) { }
                            return null;
                        })
                        .Where(x => x != null)
                        .ToList();
        }

        public static int GetChildProcessCount(this Process process)
        {
            return new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={process.Id}")
                        .Get()
                        .Cast<ManagementObject>()
                        .Select(mo =>
                            {
                                try
                                {
                                    return Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]));
                                }
                                catch (Exception) { }
                                return null;
                            })
                        .Where(x => x != null)
                        .ToList()
                        .Count;
        }
    }
}

using Discord_Bot.Communication;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace Discord_Bot.Tools
{
    public static class ProcessTools
    {
        public static async Task<ProcessMetrics> GetStatistics()
        {
            Process process = Process.GetCurrentProcess();
            PerformanceCounter total_cpu = new("Processor Information", "% Processor Time", "_Total");
            PerformanceCounter available_ram = new("Memory", "Available MBytes");
            PerformanceCounter cpu = new("Process", "% Processor Time", process.ProcessName, true);
            PerformanceCounter ram = new("Process", "Private Bytes", process.ProcessName, true); //Will show the RAM that cannot be allocated to other processes, 'Working Set - Private' will show the currently used bytes, as shown in Task Manager
            cpu.NextValue();
            ram.NextValue();
            total_cpu.NextValue();
            available_ram.NextValue();

            await Task.Delay(500);

            int processorCount = Environment.ProcessorCount;
            double total_ram = new ComputerInfo().TotalPhysicalMemory / 1024.0 / 1024.0;
            double cpu_usage = cpu.NextValue();
            double ram_usage = ram.NextValue();
            double total_cpu_usage = total_cpu.NextValue();
            double total_ram_usage = available_ram.NextValue();
            int childProcessCount = process.GetChildProcessCount();
            ProcessMetrics result = new()
            {
                CPUUsagePercent = Math.Round(cpu_usage / processorCount, 2), 
                RAMUsageInMB = Math.Round(ram_usage / 1024 / 1024, 2),
                TotalCPUUsagePercent = Math.Round(total_cpu_usage, 2),
                TotalRAMUsagePercent = Math.Round((total_ram - total_ram_usage) / total_ram * 100, 2),
                ChildProcessCount = childProcessCount
            };

            return result;
        }

        public static IList<Process> GetChildProcesses(this Process process)
        {
            return new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={process.Id}")
                        .Get()
                        .Cast<ManagementObject>()
                        .Select(mo =>
                            Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])))
                        .ToList();
        }

        public static int GetChildProcessCount(this Process process)
        {
            return new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={process.Id}")
                        .Get()
                        .Cast<ManagementObject>()
                        .Select(mo =>
                            Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])))
                        .ToList()
                        .Count;
        }
    }
}

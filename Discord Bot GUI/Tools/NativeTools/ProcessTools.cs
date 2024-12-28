using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Tools.Extensions;
using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Tools.NativeTools;

public static class ProcessTools
{
    public static async Task<ProcessMetrics> GetStatistics()
    {
        Process process = Process.GetCurrentProcess();
        PerformanceCounter total_cpu = new("Processor Information", "% Processor Time", "_Total");
        PerformanceCounter available_ram = new("Memory", "Available MBytes");
        PerformanceCounter cpu = new("Process", "% Processor Time", process.ProcessName, true);
        PerformanceCounter reservedRam = new("Process", "Private Bytes", process.ProcessName, true);
        PerformanceCounter ram = new("Process", "Working Set - Private", process.ProcessName, true); //This is what is shown in task manager
        cpu.NextValue();
        reservedRam.NextValue();
        ram.NextValue();
        total_cpu.NextValue();
        available_ram.NextValue();

        await Task.Delay(500);

        int processorCount = Environment.ProcessorCount;
        int threadCount = GetActiveThreads();
        double total_ram = new ComputerInfo().TotalPhysicalMemory / 1024.0 / 1024.0;
        double cpu_usage = cpu.NextValue();
        double ram_usage = ram.NextValue();
        double reserved_ram = reservedRam.NextValue();
        double total_cpu_usage = total_cpu.NextValue();
        double total_ram_usage = available_ram.NextValue();
        int childProcessCount = process.GetChildProcessCount();
        ProcessMetrics result = new()
        {
            CPUUsagePercent = Math.Round(cpu_usage / processorCount, 2),
            RAMUsageInMB = Math.Round(ram_usage / 1024 / 1024, 2),
            ReservedRAMInMB = Math.Round(reserved_ram / 1024 / 1024, 2),
            TotalCPUUsagePercent = Math.Round(total_cpu_usage, 2),
            TotalRAMUsagePercent = Math.Round((total_ram - total_ram_usage) / total_ram * 100, 2),
            ThreadCount = threadCount,
            ChildProcessCount = childProcessCount
        };

        return result;
    }

    private static int GetActiveThreads()
    {
        ProcessThreadCollection threads = Process.GetCurrentProcess().Threads;
        int threadCount = 0;
        for (int i = 0; i < threads.Count; i++)
        {
            if (Constant.ActiveThreadStates.Contains(threads[i].ThreadState))
            {
                threadCount++;
            }
        }

        return threadCount;
    }
}

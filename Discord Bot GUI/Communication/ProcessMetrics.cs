namespace Discord_Bot.Communication
{
    public class ProcessMetrics
    {
        public double CPUUsagePercent { get; set; }
        public double RAMUsageInMB { get; set; }
        public double TotalCPUUsagePercent { get; set; }
        public double TotalRAMUsagePercent { get; set; }
        public int ChildProcessCount { get; set; }
        public int ThreadCount { get; internal set; }
    }
}

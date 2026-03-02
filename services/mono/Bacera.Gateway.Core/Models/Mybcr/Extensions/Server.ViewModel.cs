using System.Text.Json.Serialization;

namespace Bacera.Gateway;

using M = Server;

public partial class Server
{
    public class TenantPageModel
    {
        public ulong Id { get; set; }
        public string? Region { get; set; }
        public string Name { get; set; } = null!;

        public string? Instance { get; set; }
        public string Ip { get; set; } = null!;

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string System { get; set; } = null!;
        public string Stat { get; set; } = null!;
        public sbyte Status { get; set; }

        public object Metrics { get; set; } = new();
    }

    public sealed class TenantServerMetricModel
    {
        public string CpuJson { get; set; } = "{}";
        public string LoadJson { get; set; } = "{}";
        public string MemoryJson { get; set; } = "{}";
        public string DiskJson { get; set; } = "{}";
    }
}

public static class ServerViewModelExt
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> q) => q
        .Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            Region = x.Region,
            Name = x.Name,
            Instance = x.Instance,
            Ip = x.Ip,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            System = x.Provider,
            Stat = x.Stat,
            Status = x.Status,
        });
}
//
// public sealed class ServerMetricModel
// {
//     public CpuPerformanceMetrics Cpu { get; set; } = new();
//
//     public LoadAverageMetrics LoadAverage { get; set; } = new();
//
//     public MemoryMetrics Memory { get; set; } = new();
//
//     public List<DiskPartitionMetrics> DiskPartitions { get; set; } = new();
// }
//
// public sealed class CpuPerformanceMetrics
// {
//     [JsonPropertyName("total")] public double Total { get; set; }
//
//     [JsonPropertyName("idle")] public double Idle { get; set; }
//     [JsonPropertyName("user")] public double User { get; set; }
//
//     [JsonPropertyName("nice")] public double Nice { get; set; }
//
//     [JsonPropertyName("system")] public double System { get; set; }
//
//     [JsonPropertyName("iowait")] public double IoWait { get; set; }
//
//     [JsonPropertyName("irq")] public double Irq { get; set; }
//
//     [JsonPropertyName("steal")] public double Steal { get; set; }
//
//     [JsonPropertyName("guest")] public double Guest { get; set; }
//
//     [JsonPropertyName("ctx_switches")] public long CtxSwitches { get; set; }
//
//     [JsonPropertyName("interrupts")] public long Interrupts { get; set; }
//
//     [JsonPropertyName("soft_interrupts")] public long SoftInterrupts { get; set; }
//
//     [JsonPropertyName("syscalls")] public long Syscalls { get; set; }
//
//     [JsonPropertyName("cpucore")] public int CpuCore { get; set; }
//
//     [JsonPropertyName("time_since_update")]
//     public double TimeSinceUpdate { get; set; }
//
//     [JsonPropertyName("ctx_switches_gauge")]
//     public long CtxSwitchesGauge { get; set; }
//
//     [JsonPropertyName("ctx_switches_rate_per_sec")]
//     public int CtxSwitchesRatePerSec { get; set; }
//
//     [JsonPropertyName("interrupts_gauge")] public long InterruptsGauge { get; set; }
//
//     [JsonPropertyName("interrupts_rate_per_sec")]
//     public int InterruptsRatePerSec { get; set; }
//
//     [JsonPropertyName("soft_interrupts_gauge")]
//     public long SoftInterruptsGauge { get; set; }
//
//     [JsonPropertyName("soft_interrupts_rate_per_sec")]
//     public int SoftInterruptsRatePerSec { get; set; }
//
//     [JsonPropertyName("syscalls_gauge")] public long SyscallsGauge { get; set; }
// }
//
// public sealed class LoadAverageMetrics
// {
//     [JsonPropertyName("min1")] public double Min1 { get; set; }
//
//     [JsonPropertyName("min5")] public double Min5 { get; set; }
//
//     [JsonPropertyName("min15")] public double Min15 { get; set; }
//
//     [JsonPropertyName("cpucore")] public int CpuCore { get; set; }
// }
//
// public sealed class MemoryMetrics
// {
//     [JsonPropertyName("total")] public long Total { get; set; }
//
//     [JsonPropertyName("available")] public long Available { get; set; }
//
//     [JsonPropertyName("percent")] public double Percent { get; set; }
//
//     [JsonPropertyName("used")] public long Used { get; set; }
//
//     [JsonPropertyName("free")] public long Free { get; set; }
//
//     [JsonPropertyName("active")] public long Active { get; set; }
//
//     [JsonPropertyName("inactive")] public long Inactive { get; set; }
//
//     [JsonPropertyName("buffers")] public long Buffers { get; set; }
//
//     [JsonPropertyName("cached")] public long Cached { get; set; }
//
//     [JsonPropertyName("shared")] public long Shared { get; set; }
// }
//
// public sealed class DiskPartitionMetrics
// {
//     [JsonPropertyName("device_name")] public string DeviceName { get; set; } = string.Empty;
//
//     [JsonPropertyName("fs_type")] public string FsType { get; set; } = string.Empty;
//
//     [JsonPropertyName("mnt_point")] public string MntPoint { get; set; } = string.Empty;
//
//     [JsonPropertyName("size")] public long Size { get; set; } = 0;
//
//     [JsonPropertyName("used")] public long Used { get; set; } = 0;
//
//     [JsonPropertyName("free")] public long Free { get; set; } = 0;
//
//     [JsonPropertyName("percent")] public double Percent { get; set; } = 0.0;
//
//     [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
// }
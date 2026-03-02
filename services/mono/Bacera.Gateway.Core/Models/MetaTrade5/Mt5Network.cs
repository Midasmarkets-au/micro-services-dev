using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Network
{
    public ulong Login { get; set; }

    public long Timestamp { get; set; }

    public uint Type { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string AddressIpv6 { get; set; } = null!;

    public uint Port { get; set; }

    public string Adapter { get; set; } = null!;

    public uint ServiceTime { get; set; }

    public uint FailoverMode { get; set; }

    public uint FailoverTimeout { get; set; }

    public string Adapters { get; set; } = null!;

    public string Addresses { get; set; } = null!;

    public string Binds { get; set; } = null!;

    public string Points { get; set; } = null!;

    public uint Version { get; set; }

    public uint Build { get; set; }

    public string BuildDate { get; set; } = null!;

    public uint SysConnection { get; set; }

    public DateTime SysLastBoot { get; set; }

    public DateTime SysOsLastUpdate { get; set; }

    public uint SysOsVersion { get; set; }

    public uint SysOsBuild { get; set; }

    public uint SysOsFlags { get; set; }

    public DateTime SysNetDriverDate { get; set; }

    public uint SysBsodCount { get; set; }

    public DateTime SysBsodLast { get; set; }

    public DateTime SysDate { get; set; }

    public string SysOsName { get; set; } = null!;

    public string SysCpuName { get; set; } = null!;

    public uint SysCpuNumber { get; set; }

    public uint SysBits { get; set; }

    public uint SysMemoryTotal { get; set; }

    public uint SysMemoryFree { get; set; }

    public uint SysMemoryCritical { get; set; }

    public uint SysHddSize { get; set; }

    public uint SysHddFree { get; set; }

    public uint SysHddCritical { get; set; }

    public uint SysHddReadSpeed { get; set; }

    public uint SysHddReadCritical { get; set; }

    public uint SysHddWriteSpeed { get; set; }

    public uint SysHddWriteCritical { get; set; }

    public uint PerfConnectsMax { get; set; }

    public uint PerfConnectsCritical { get; set; }

    public uint PerfCpuMax { get; set; }

    public uint PerfCpuCritical { get; set; }

    public uint PerfMemoryMin { get; set; }

    public uint PerfMemoryCritical { get; set; }

    public uint PerfMemBlockMin { get; set; }

    public uint PerfMemBlockCritical { get; set; }

    public uint PerfNetworkMax { get; set; }

    public uint PerfNetworkCritical { get; set; }

    public uint PerfSocketsMax { get; set; }

    public uint PerfSocketsCritical { get; set; }
}

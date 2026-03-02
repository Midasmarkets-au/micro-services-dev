using System;
using System.Collections.Generic;

namespace Bacera.Gateway;

public partial class Server
{
    public ulong Id { get; set; }

    /// <summary>
    /// eg. aws do
    /// </summary>
    public string Provider { get; set; } = null!;

    /// <summary>
    /// eg. ec2 rds ...
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// eg. ap-east-1
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// eg. ap-east-1
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// eg. i-0ae40f12f7ec1cac5
    /// </summary>
    public string? Instanceid { get; set; }

    /// <summary>
    /// eg. t3.small
    /// </summary>
    public string? Instance { get; set; }

    /// <summary>
    /// eg. 18.163.55.174
    /// </summary>
    public string Ip { get; set; } = null!;

    /// <summary>
    /// eg. 1.0.0.01
    /// </summary>
    public string? Eip { get; set; }

    /// <summary>
    /// eg. it@bacera.com
    /// </summary>
    public string Username { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Sudo { get; set; }

    public string? Creator { get; set; }

    public string System { get; set; } = null!;

    public string Stat { get; set; } = null!;

    public DateTime? LastUpdate { get; set; }

    public sbyte Status { get; set; }
}
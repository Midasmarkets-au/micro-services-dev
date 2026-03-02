using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5User
{
    public ulong Login { get; set; }

    public long Timestamp { get; set; }

    public string Group { get; set; } = null!;

    public ulong CertSerialNumber { get; set; }

    public ulong Rights { get; set; }

    public DateTime Registration { get; set; }

    public DateTime LastAccess { get; set; }

    public DateTime LastPassChange { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string MiddleName { get; set; } = null!;

    public string Company { get; set; } = null!;

    public string Account { get; set; } = null!;

    public string Country { get; set; } = null!;

    public uint Language { get; set; }

    public ulong ClientId { get; set; }

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string ZipCode { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Id { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Comment { get; set; } = null!;

    public uint Color { get; set; }

    public string PhonePassword { get; set; } = null!;

    public uint Leverage { get; set; }

    public ulong Agent { get; set; }

    public string TradeAccounts { get; set; } = null!;

    public double LimitPositions { get; set; }

    public uint LimitOrders { get; set; }

    public string LeadCampaign { get; set; } = null!;

    public string LeadSource { get; set; } = null!;

    public long TimestampTrade { get; set; }

    public double Balance { get; set; }

    public double Credit { get; set; }

    public double InterestRate { get; set; }

    public double CommissionDaily { get; set; }

    public double CommissionMonthly { get; set; }

    public double BalancePrevDay { get; set; }

    public double BalancePrevMonth { get; set; }

    public double EquityPrevDay { get; set; }

    public double EquityPrevMonth { get; set; }

    public string Name { get; set; } = null!;

    public string Mqid { get; set; } = null!;

    public string LastIp { get; set; } = null!;

    public string ApiData { get; set; } = null!;
}

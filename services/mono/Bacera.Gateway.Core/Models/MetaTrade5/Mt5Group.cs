using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Group
{
    public ulong GroupId { get; set; }

    public string Group { get; set; } = null!;

    public long Timestamp { get; set; }

    public ulong Server { get; set; }

    public ulong PermissionsFlags { get; set; }

    public uint AuthMode { get; set; }

    public uint AuthPasswordMin { get; set; }

    public uint AuthOtpmode { get; set; }

    public string Company { get; set; } = null!;

    public string CompanyPage { get; set; } = null!;

    public string CompanyEmail { get; set; } = null!;

    public string CompanySupportPage { get; set; } = null!;

    public string CompanySupportEmail { get; set; } = null!;

    public string CompanyCatalog { get; set; } = null!;

    public string CompanyDepositUrl { get; set; } = null!;

    public string CompanyWithdrawalUrl { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public uint CurrencyDigits { get; set; }

    public uint ReportsMode { get; set; }

    public string ReportsEmail { get; set; } = null!;

    public ulong ReportsFlags { get; set; }

    public uint NewsMode { get; set; }

    public string NewsCategory { get; set; } = null!;

    public uint MailMode { get; set; }

    public ulong TradeFlags { get; set; }

    public uint TransferMode { get; set; }

    public double TradeInterestrate { get; set; }

    public double TradeVirtualCredit { get; set; }

    public uint MarginMode { get; set; }

    public uint MarginSomode { get; set; }

    public uint MarginFreeMode { get; set; }

    public double MarginCall { get; set; }

    public double MarginStopOut { get; set; }

    public uint MarginFreeProfitMode { get; set; }

    public uint DemoLeverage { get; set; }

    public double DemoDeposit { get; set; }

    public uint DemoTradesClean { get; set; }

    public uint LimitHistory { get; set; }

    public uint LimitOrders { get; set; }

    public uint LimitSymbols { get; set; }

    public uint LimitPositions { get; set; }

    public double LimitPositionsVolume { get; set; }
}

using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt4User
{
    public int Login { get; set; }

    public string Group { get; set; } = null!;

    public int Enable { get; set; }

    public int EnableChangePass { get; set; }

    public int EnableReadonly { get; set; }

    public int EnableOtp { get; set; }

    public string PasswordPhone { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string Zipcode { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string LeadSource { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Comment { get; set; } = null!;

    public string Id { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime Regdate { get; set; }

    public DateTime Lastdate { get; set; }

    public int Leverage { get; set; }

    public int AgentAccount { get; set; }

    public int Timestamp { get; set; }

    public double Balance { get; set; }

    public double Prevmonthbalance { get; set; }

    public double Prevbalance { get; set; }

    public double Credit { get; set; }

    public double Interestrate { get; set; }

    public double Taxes { get; set; }

    public int SendReports { get; set; }

    public uint Mqid { get; set; }

    public int UserColor { get; set; }

    public double Equity { get; set; }

    public double Margin { get; set; }

    public double MarginLevel { get; set; }

    public double MarginFree { get; set; }

    public string Currency { get; set; } = null!;

    public byte[]? ApiData { get; set; }

    public DateTime ModifyTime { get; set; }
}

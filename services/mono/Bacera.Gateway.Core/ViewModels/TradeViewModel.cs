namespace Bacera.Gateway;

public partial class TradeViewModel
{
    public long Id { get; set; }
    public long TenantId { get; set; }

    public long AccountNumber { get; set; }

    public int ServiceId { get; set; }

    public long Ticket { get; set; }

    public string Symbol { get; set; } = null!;
    public long? Position { get; set; }

    public int Digits { get; set; }

    public int Cmd { get; set; }

    public double Volume { get; set; }

    public DateTime? OpenAt { get; set; }

    public double? OpenPrice { get; set; }

    public double Sl { get; set; }

    public double Tp { get; set; }

    public double Price => ClosePrice ?? OpenPrice ?? 0;

    public DateTime? CloseAt { get; set; }

    public double? ClosePrice { get; set; }

    // public DateTime? ExpiresAt { get; set; }

    public int Reason { get; set; }
    //
    // public double ConvertRate { get; set; }
    //
    // public double ConvertRate2 { get; set; }
    //
    // public double Commission { get; set; }
    //
    // public double CommissionAgent { get; set; }
    //
    // public double Swaps { get; set; }

    public double Profit { get; set; }

    // public double Taxes { get; set; }

    public string Comment { get; set; } = null!;
    //
    // public double MarginRate { get; set; }
    //
    // public DateTime ModifiedAt { get; set; }
    //
    // public short Status { get; set; }

    public long TimeStamp { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }
    public double CurrentPrice { get; set; }
    public double Commission { get; set; }
    public double Swaps { get; set; }
    public string NativeName { get; set; } = "";
    public string Email { get; set; } = "";
}
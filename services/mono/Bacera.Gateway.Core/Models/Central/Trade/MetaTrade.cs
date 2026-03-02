namespace Bacera.Gateway;

public partial class MetaTrade
{
    public long Id { get; set; }
    public long TenantId { get; set; }

    public long AccountNumber { get; set; }

    public int ServiceId { get; set; }

    public long Ticket { get; set; }

    public string Symbol { get; set; } = null!;
    public int Cmd { get; set; }

    public DateTime? OpenAt { get; set; }
    public DateTime? CloseAt { get; set; }
    public long TimeStamp { get; set; }

    public long? Position { get; set; }

    public int Digits { get; set; }


    public double Volume { get; set; }
    public int VolumeOriginal { get; set; }

    public double? OpenPrice { get; set; }

    public double Sl { get; set; }

    public double Tp { get; set; }


    public double? ClosePrice { get; set; }


    public int Reason { get; set; }

    public double Profit { get; set; }

    public string Comment { get; set; } = "";


    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }
    public double CurrentPrice { get; set; }
    public double Commission { get; set; }
    public double Swaps { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
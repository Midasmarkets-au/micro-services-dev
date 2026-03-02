using Bacera.Gateway.Interfaces;

namespace Bacera.Gateway.Services.Report.Models;

public class DemoAccountRecord : ICanExportToCsv, IEntity
{
    public long PartyId { get; set; }
    public long Uid { get; set; }
    public long AccountNumber { get; set; }
    public DateTime ExpireOn { get; set; }
    public int Leverage { get; set; }
    public decimal Balance { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }

    public static string Header()
        =>
            "AccountNumber,ExpireOn,Leverage,Balance,Type,Currency,NativeName,FirstName,LastName,CountryCode,PhoneNumber,Email,Language,CreatedOn";

    public string ToCsv()
        =>
            $"{AccountNumber},{ExpireOn:yyyy-MM-dd HH:mm:ss},{Leverage},{Balance / 100m},{Type},{Currency},{NativeName},{FirstName},{LastName},{CountryCode},{PhoneNumber},{Email},{Language},{CreatedOn:yyyy-MM-dd HH:mm:ss}";

    public sealed class QueryCriteria : EntityCriteria<DemoAccountRecord>
    {
        public DateTime Date { get; set; }

        protected override void OnCollect(ICriteriaPool<DemoAccountRecord> pool)
        {
            //
        }
    }

    public long Id { get; set; }
}
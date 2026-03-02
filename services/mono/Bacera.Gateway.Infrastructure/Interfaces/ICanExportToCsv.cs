namespace Bacera.Gateway.Interfaces;

public interface ICanExportToCsv : IHasPartyId
{
    string ClientName { get; set; }
    static abstract string Header();
    string ToCsv();
}
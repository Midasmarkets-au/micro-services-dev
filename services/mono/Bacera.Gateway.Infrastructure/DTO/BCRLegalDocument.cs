using Newtonsoft.Json;

namespace Bacera.Gateway.DTO;

public class BCRLegalDocument
{
    public string Title { get; set; } = null!;
    public string Src { get; set; } = null!;

    public static List<BCRLegalDocument> ParseAsList(string json)
        => JsonConvert.DeserializeObject<List<BCRLegalDocument>>(json)!;
}

public sealed class BCRLegalDocumentData
{
    public string BaseUrl { get; set; } = null!;
    public List<BCRLegalDocument> Documents { get; set; } = null!;

    public static BCRLegalDocumentData Parse(string json)
        => JsonConvert.DeserializeObject<BCRLegalDocumentData>(json)!;
}
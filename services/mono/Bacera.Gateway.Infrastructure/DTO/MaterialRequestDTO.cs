using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bacera.Gateway.Web;

[Serializable]
public class MaterialRequestContentDTO
{
    [JsonProperty("materialType")]
    [JsonPropertyName("materialType")]
    [Required]
    public string MaterialType { get; set; } = null!;

    [JsonProperty("description")]
    [JsonPropertyName("description")]
    [Required]
    public string Description { get; set; } = null!;

    [JsonProperty("quantity")]
    [JsonPropertyName("quantity")]
    public int? Quantity { get; set; }

    [JsonProperty("attachments")]
    [JsonPropertyName("attachments")]
    public List<MaterialAttachmentDTO> Attachments { get; set; } = [];
}

public class MaterialAttachmentDTO
{
    [JsonProperty("guid")]
    [JsonPropertyName("guid")]
    public string Guid { get; set; } = string.Empty;

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonProperty("fileName")]
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;
}

public class MaterialRequestCreateModel
{
    [Required]
    public string MaterialType { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    public int? Quantity { get; set; }
}

public class MaterialRequestRejectModel
{
    [Required]
    public string Note { get; set; } = null!;
}

public class MaterialRequestDTO
{
    public long Id { get; set; }
    public long PartyId { get; set; }
    public int Status { get; set; } = (int)VerificationStatusTypes.AwaitingReview;
    public DateTime? CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string Note { get; set; } = string.Empty;
    public MaterialRequestContentDTO Content { get; set; } = new();
    public TenantUserBasicModel User { get; set; } = new();

    public static MaterialRequestDTO FromEntity(MaterialRequest request)
    {
        List<MaterialAttachmentDTO> attachments;
        try
        {
            attachments = JsonConvert.DeserializeObject<List<MaterialAttachmentDTO>>(request.Attachments) ?? [];
        }
        catch
        {
            attachments = [];
        }

        return new MaterialRequestDTO
        {
            Id = request.Id,
            PartyId = request.PartyId,
            Status = request.Status,
            CreatedOn = request.CreatedOn,
            UpdatedOn = request.UpdatedOn,
            Note = request.Note,
            Content = new MaterialRequestContentDTO
            {
                MaterialType = request.MaterialType,
                Description = request.Description,
                Quantity = request.Quantity,
                Attachments = attachments,
            },
        };
    }
}

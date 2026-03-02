using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace Bacera.Gateway.Web;
[Serializable]
public class SetLanguageRequestModel
{
    [JsonProperty("language")]
    [JsonPropertyName("language")]
    [RegularExpression(LanguageTypes.AllLanguageRegEx)]
    [Required]
    public string Language { get; set; } = null!;
}

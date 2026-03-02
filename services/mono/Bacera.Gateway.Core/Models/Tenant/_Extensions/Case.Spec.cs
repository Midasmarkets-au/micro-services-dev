using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class Case
{
    public class TenantCreateSpec
    {
        public long PartyId { get; set; }
        public long CategoryId { get; set; }
        public string Content { get; set; } = "";
        public object Data { get; set; } = new();
        public string? SourceLanguage { get; set; }
        public string? TargetLanguage { get; set; }
        public List<string> MediumGuids { get; set; } = new();

        public Case ToEntity()
            => new()
            {
                PartyId = PartyId,
                CategoryId = CategoryId,
                Content = Content,
                Data = JsonConvert.SerializeObject(Data),
                Files = ToJsonArray(MediumGuids),
                Status = (short)CaseStatusTypes.Created,
            };
    }

    public class TenantTranslateSpec
    {
        [Required]
        [MinLength(1, ErrorMessage = "TargetLanguage cannot be empty")]
        public string TargetLanguage { get; set; } = null!;
    }

    public class ReplySpec
    {
        public string Content { get; set; } = "";
        public object Data { get; set; } = new();
        public string? SourceLanguage { get; set; }
        public string? TargetLanguage { get; set; }
        public List<string> MediumGuids { get; set; } = new();

        public Case ToEntity(long operatorPartyId = 1, bool isAdmin = true)
            => new()
            {
                Content = Content,
                PartyId = operatorPartyId,
                IsAdmin = isAdmin,
                Data = JsonConvert.SerializeObject(Data),
                Files = ToJsonArray(MediumGuids),
                Status = (short)CaseStatusTypes.Processing,
            };
    }

    public class ClientCreateSpec
    {
        public long CategoryId { get; set; }
        public string Content { get; set; } = "";
        public object Data { get; set; } = new();
        public List<string> MediumGuids { get; set; } = new();

        public Case ToEntity()
            => new()
            {
                CategoryId = CategoryId,
                Content = Content,
                Data = JsonConvert.SerializeObject(Data),
                Files = ToJsonArray(MediumGuids),
                Status = (short)CaseStatusTypes.Created,
            };
    }

    public static string ToJsonArray(IEnumerable<string> strings)
        => "[" + string.Join(",", strings.Select(guid => $"\"{guid}\"")) + "]";
}
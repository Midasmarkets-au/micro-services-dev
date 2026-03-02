using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using FluentValidation;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class Lead
{
    public bool IsEmpty() => Id == 0;
    public bool HasPartyMatched() => PartyId != null && PartyId != 0;

    public Dictionary<string, LeadItem> GetSupplement() =>
        Utils.JsonDeserializeObjectWithDefault<Dictionary<string, LeadItem>>(Supplement);

    public string GetUtm() => GetSupplement().GetValueOrDefault("utm")?.Data ?? string.Empty;

    public static Lead Build(
        long partyId = 0
        , string name = ""
        , string email = ""
        , string phoneNumber = ""
        , Dictionary<string, LeadItem>? supplement = null
        , LeadStatusTypes status = LeadStatusTypes.UserNotRegistered
        , LeadSourceTypes sourceType = LeadSourceTypes.ManuallyAdd
    )
        => new()
        {
            PartyId = partyId,
            IsArchived = (int)LeadIsArchivedTypes.Unarchived,
            Name = name,
            PhoneNumber = phoneNumber,
            Email = email,
            Supplement = Utils.JsonSerializeObject(supplement ?? new Dictionary<string, LeadItem>()),
            Status = (int)status,
            SourceType = (int)sourceType,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };
}
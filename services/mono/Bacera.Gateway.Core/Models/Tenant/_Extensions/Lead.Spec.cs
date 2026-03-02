using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using FluentValidation;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class Lead
{
    public class CreateSpec
    {
        public long PartyId { get; set; }
        public string Name { get; set; } = string.Empty;

        [Required] public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
        public LeadStatusTypes Status { get; set; } = LeadStatusTypes.UserNotRegistered;
        public LeadSourceTypes SourceType { get; set; } = LeadSourceTypes.ManuallyAdd;
        public Dictionary<string, LeadItem>? Supplement { get; set; } = new();

        public string? SourceComment { get; set; }

        public static CreateSpec Build(
            long partyId = 0
            , string name = ""
            , string email = ""
            , string phoneNumber = ""
            , LeadSourceTypes sourceType = LeadSourceTypes.ManuallyAdd
            , LeadStatusTypes status = LeadStatusTypes.UserNotRegistered
            , Dictionary<string, LeadItem>? supplement = null
        )
            => new()
            {
                PartyId = partyId,
                Name = name,
                PhoneNumber = phoneNumber,
                Email = email,
                Status = status,
                SourceType = sourceType,
                Supplement = supplement ?? new Dictionary<string, LeadItem>(),
            };
    }

    public class AddCommentSpec
    {
        [Required] public string Content { get; set; } = string.Empty;
    }
}

public class LeadCreateSpecValidator : AbstractValidator<Lead.CreateSpec>
{
    public LeadCreateSpecValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Name).NotEmpty();
    }
}
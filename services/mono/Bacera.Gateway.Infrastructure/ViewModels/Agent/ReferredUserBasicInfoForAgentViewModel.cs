using Bacera.Gateway.Interfaces;
using Bacera.Gateway.ViewModels.Parent;

namespace Bacera.Gateway.Agent;

public class ReferredUserBasicInfoForAgentViewModel
{
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public long PartyId { get; set; }

    public ParentUserBasicModel User { get; set; } = ParentUserBasicModel.Empty();
    public ReferredUserVerificationViewModel Verification { get; set; } = ReferredUserVerificationViewModel.Empty();
}

public static class AgentVerificationBasicViewModelExtension
{
    public static IQueryable<ReferredUserBasicInfoForAgentViewModel> ToAgentViewModel(this IQueryable<Referral> query)
        => query.Select(x => new
            {
                x.ReferredPartyId,
                x.ReferredParty,
                Verification = x.ReferredParty.Verifications.FirstOrDefault(v => v.Type == (int)VerificationTypes.Verification)
            })
            .Select(y => new ReferredUserBasicInfoForAgentViewModel
            {
                PartyId = y.ReferredPartyId,
                Verification = y.Verification != null
                    ? new ReferredUserVerificationViewModel
                    {
                        UpdatedOn = y.Verification.UpdatedOn,
                        Status = (VerificationStatusTypes)y.Verification.Status
                    }
                    : new ReferredUserVerificationViewModel(),
                User = y.ReferredParty.ToParentBasicViewModel()
            });
}
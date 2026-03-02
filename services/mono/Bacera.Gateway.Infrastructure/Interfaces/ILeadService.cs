using Bacera.Gateway.Core.Types;
using Bacera.Gateway.ViewModels.Tenant;

namespace Bacera.Gateway;

public interface ILeadService
{
    Task<Result<List<LeadBasicViewModel>, Lead.Criteria>> QueryViewModelAsync(Lead.Criteria criteria);
    Task<LeadDetailViewModel> GetAsync(long id);
    Task<LeadDetailViewModel> LookUpUnderAssignedAccountUid(long assignedAccountUid, long id);

    Task<bool> TryReferenceTo(long partyId, string email = "", string phoneNumber = "", string name = "",
        LeadStatusTypes status = LeadStatusTypes.UserRegistered, string? sourceComment = null, string? utm = null);

    Task<Lead> CreateAsync(Lead.CreateSpec spec);

    Task<bool> AppendEvent<T, TC>(long partyId, ILeadable<T, TC> entity, LeadStatusTypes status)
        where T : struct, IConvertible
        where TC : struct, IConvertible;

    Task<bool> UpdateStatus(long id, LeadStatusTypes status);
    Task<bool> AssignOwnerAccount(long leadId, long assignedAccountUid, long operatorPartyId);
    Task<bool> UnAssignOwnerAccount(long leadId, long assignedAccountUid, long operatorPartyId);
    Task<bool> Archive(long id, LeadIsArchivedTypes archivedType);

    Task<bool> AddComment(long id, string content, long operatorPartyId);
}
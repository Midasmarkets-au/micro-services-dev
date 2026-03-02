using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

using Newtonsoft.Json;
using M = Application;

partial class Application : ILeadable<short>
{
    public bool IsEmpty() => Id == 0;

    public static Application Build(long partyId, ApplicationTypes type, long referenceId = 0,
        ApplicationStatusTypes status = ApplicationStatusTypes.AwaitingApproval) => new()
    {
        PartyId = partyId,
        Type = (short)type,
        ReferenceId = referenceId,
        Status = (short)status,
    };

    public bool CanUpdate() => Status == (short)ApplicationStatusTypes.AwaitingApproval;
    public bool CanApprove() => Status == (short)ApplicationStatusTypes.AwaitingApproval;
    public bool CanReject() => Status == (short)ApplicationStatusTypes.AwaitingApproval;
    public bool CanReverseReject() => Status == (short)ApplicationStatusTypes.Rejected;
    public bool CanComplete() => Status == (short)ApplicationStatusTypes.Approved;

    public bool Complete(long operatorPartyId)
    {
        if (!CanComplete()) return false;
        CompletedBy = operatorPartyId;
        CompletedOn = DateTime.UtcNow;
        UpdatedOn = DateTime.UtcNow;
        Status = (short)ApplicationStatusTypes.Completed;
        return true;
    }

    public bool Approve(long operatorPartyId, ApplicationTypes type, long referenceId = 0)
    {
        if (!CanApprove()) return false;
        ApprovedBy = operatorPartyId;
        ApprovedOn = DateTime.UtcNow;
        UpdatedOn = DateTime.UtcNow;
        Type = (short)type;
        Status = (short)ApplicationStatusTypes.Approved;
        if (referenceId > 0) ReferenceId = referenceId;
        return true;
    }

    [Serializable]
    public class RejectRequestModel
    {
        [StringLength(255)] public string Reason { get; set; } = "";
        public static RejectRequestModel Build(string reason = "") => new() { Reason = reason };
    }

    public bool Reject(long operatorPartyId, string message = "")
    {
        if (!CanReject()) return false;
        RejectedBy = operatorPartyId;
        RejectedOn = DateTime.UtcNow;
        UpdatedOn = DateTime.UtcNow;
        RejectedReason = message;
        Status = (short)ApplicationStatusTypes.Rejected;
        return true;
    }

    public bool ReverseReject(long operatorPartyId, string message = "")
    {
        if (!CanReverseReject()) return false;
        RejectedBy = operatorPartyId;
        RejectedOn = DateTime.UtcNow;
        UpdatedOn = DateTime.UtcNow;
        RejectedReason = message;
        Status = (short)ApplicationStatusTypes.AwaitingApproval;
        return true;
    }

    public class ResponseModel
    {
        public long Id { get; set; }
        public long PartyId { get; set; }
        public short Type { get; set; }

        public DateTime? ApprovedOn { get; set; }

        public long? ApprovedBy { get; set; }

        public DateTime? RejectedOn { get; set; }

        public long? RejectedBy { get; set; }

        public string? RejectedReason { get; set; }

        public long? ReferenceId { get; set; }
        public short? Status { get; set; }

        public long? CompletedBy { get; set; }

        public DateTime? CompletedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public object Supplement => Utils.JsonDeserializeDynamic(SupplementData ?? "{}");

        [JsonIgnore] public string? SupplementData { get; set; }
        public SiteTypes? SiteId { get; set; }

        [JsonIgnore] public dynamic UserRaw { get; set; } = null!;

        [JsonIgnore] public bool HasComment { get; set; }

        // public dynamic User => new
        // {
        //     UserRaw.Id,
        //     UserRaw.PartyId,
        //     UserRaw.Email,
        //     UserRaw.Avatar,
        //     UserRaw.FirstName,
        //     UserRaw.LastName,
        //     UserRaw.NativeName,
        //     UserRaw.PartyTags,
        //     HasComment,
        // };

        public TenantUserBasicModel User { get; set; } = null!;
    }
}
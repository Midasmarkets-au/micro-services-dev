namespace Bacera.Gateway;

using M = MessageRecord;

partial class MessageRecord
{
    public class TenantPageModel
    {
        public long Id { get; set; }

        public string Method { get; set; } = string.Empty;

        public string Receiver { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;

        public long ReceiverPartyId { get; set; }

        public string Event { get; set; } = string.Empty;

        public long EventId { get; set; }

        public StatusTypes Status { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }

    public class TenantDetailModel : TenantPageModel
    {
        public string? Content { get; set; }
    }
}

public static class MessageRecordViewModelExtensions
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> q) => q.Select(x => new M.TenantPageModel
    {
        Id = x.Id,
        Method = x.Method,
        Receiver = x.Receiver,
        ReceiverName = x.ReceiverParty.NativeName,
        ReceiverPartyId = x.ReceiverPartyId,
        Event = x.Event,
        EventId = x.EventId,
        Status = (M.StatusTypes)x.Status,
        Note = x.Note,
        CreatedOn = x.CreatedOn,
        UpdatedOn = x.UpdatedOn
    });

    public static IQueryable<M.TenantDetailModel> ToTenantDetailModel(this IQueryable<M> q) => q.Select(x => new M.TenantDetailModel
    {
        Id = x.Id,
        Method = x.Method,
        Receiver = x.Receiver,
        ReceiverName = x.ReceiverParty.NativeName,
        ReceiverPartyId = x.ReceiverPartyId,
        Content = x.Content,
        Event = x.Event,
        EventId = x.EventId,
        Status = (M.StatusTypes)x.Status,
        Note = x.Note,
        CreatedOn = x.CreatedOn,
        UpdatedOn = x.UpdatedOn
    });
}
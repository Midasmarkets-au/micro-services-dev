using M = Bacera.Gateway.CommunicateHistory;

namespace Bacera.Gateway;

partial class CommunicateHistory
{
    public class ResponseModel
    {
        public long Id { get; set; }
        public CommunicateTypes Type { get; set; }
        public long PartyId { get; set; }
        public long RowId { get; set; }
        public long OperatorPartyId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }

    public static M Build(long partyId, long rowId, CommunicateTypes type, string content, long operatorPartyId = 1) =>
        new()
        {
            PartyId = partyId,
            RowId = rowId,
            Type = (int)type,
            Content = content,
            OperatorPartyId = operatorPartyId,
            CreatedOn = DateTime.UtcNow
        };
}

public static class CommunicateHistoryExtensions
{
    public static IQueryable<CommunicateHistory.ResponseModel> ToResponseModel(
        this IQueryable<CommunicateHistory> models)
        => models.Select(x => new CommunicateHistory.ResponseModel
        {
            Id = x.Id,
            PartyId = x.PartyId,
            RowId = x.RowId,
            Type = (CommunicateTypes)x.Type,
            OperatorPartyId = x.OperatorPartyId,
            OperatorName = x.Party.Name,
            CreatedOn = x.CreatedOn
        });
}
namespace Bacera.Gateway.Vendor.MetaTrader;

using M = CopyTrade;
public interface ICopyTradeService
{
    Task<Result<List<M>, M.Criteria>> QueryAsync(M.Criteria criteria);
    Task<M> GetAsync(long id);
    Task<M> GetForPartyAsync(long id, long partyId);
    Task<Result<bool>> DeleteAsync(long id);
    Task<Result<bool>> DeleteForPartyAsync(long id, long partyId);
    Task<Result<M>> CreateAsync(long sourceAccountId, long targetAccountId, string mode, int? value = 0);
    Task<List<Rule>> ListRulesByApiAsync();
    Task<bool> DeleteRuleByApiAsync(int ruleNumber);

    Task<int> CreateRuleByApiAsync(long sourceAccountNumber, long targetAccountNumber, string mode,
        int? value = 0);
}
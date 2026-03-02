// namespace Bacera.Gateway;
//
// public class RebateAccount
// {
//     public long Id { get; set; }
//     public long? AgentAccountId { get; set; }
//     public long Uid { get; set; }
//     public long PartyId { get; set; }
//     public int Role { get; set; }
//     public int Type { get; set; }
//
//     public int Depth { get; set; }
//     public bool IsRoot() => AgentAccountId is null or 0;
//
//     public static RebateAccount From(Account agent, int depth)
//         => new()
//         {
//             Id = agent.Id, AgentAccountId = agent.AgentAccountId, Depth = depth, PartyId = agent.PartyId,
//             Role = agent.Role, Type = agent.Type, Uid = agent.Uid
//         };
// }
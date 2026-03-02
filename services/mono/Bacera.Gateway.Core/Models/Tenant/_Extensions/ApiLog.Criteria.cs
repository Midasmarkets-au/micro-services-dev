using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = ApiLog;

public partial class ApiLog : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(Id);
        }

        // [FromQuery] long partyId = 0, [FromQuery] string action = "", [FromQuery] string method = "",
        // [FromQuery] bool showBasic = true, [FromQuery] long page = 1, [FromQuery] long size = 20

        public long? PartyId { get; set; }
        public short? StatusCode { get; set; }
        public string? Action { get; set; }
        public string? Method { get; set; }
        public string? Ip { get; set; }
        public bool? ShowBasic { get; set; }


        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId != null);
            pool.Add(x => x.Action.Contains(Action!), Action != null);
            pool.Add(x => x.Method == Method, Method != null);
            pool.Add(x => x.StatusCode == StatusCode, StatusCode != null);
            pool.Add(x => x.Ip == Ip, Ip != null);
        }
    }
}
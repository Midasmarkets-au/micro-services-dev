using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = AuthCode;

public partial class AuthCode : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
        }

        public long? PartyId { get; set; }
        public string? Email { get; set; }
        public AuthCodeStatusTypes? Status { get; set; }
        public List<AuthCodeStatusTypes>? Statuses { get; set; }
        public string? Event { get; set; }
        public string? Code { get; set; }
        public AuthCodeMethodTypes? Method { get; set; }
        public string? MethodValue { get; set; }
        public bool? IsExpired { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.PartyId == PartyId, PartyId != null);
            pool.Add(x => x.Status == (short)Status!, Status != null);
            pool.Add(x => Statuses!.Contains((AuthCodeStatusTypes)x.Status), Statuses != null);
            pool.Add(x => x.Event == Event, Event != null);
            pool.Add(x => x.Code == Code, Code != null);
            pool.Add(x => x.Party.Email == Email, Email != null);
            pool.Add(x => x.Method == (int)Method!, Method != null);
            pool.Add(x => x.MethodValue == MethodValue, MethodValue != null);
            pool.Add(x => x.ExpireOn < DateTime.UtcNow, IsExpired is true);
        }
    }
}
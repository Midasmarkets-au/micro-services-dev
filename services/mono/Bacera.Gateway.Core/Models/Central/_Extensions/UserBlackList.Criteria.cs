using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

using M = Bacera.Gateway.UserBlackList;

public partial class UserBlackList : IEntity
{
    public sealed class Criteria : EntityCriteria<M>
    {
        public Criteria()
        {
            SortField = nameof(M.Id);
            SortFlag = true;
        }
        public string? OperatorName { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? IdNumber { get; set; }

        protected override void OnCollect(ICriteriaPool<M> pool)
        {
            pool.Add(x => x.Name.Contains(Name!), Name is { Length: > 1 });
            pool.Add(x => x.Phone.Contains(Phone!), Phone is { Length: > 2 });
            pool.Add(x => x.Email.Contains(Email!), Email is { Length: > 2 });
            pool.Add(x => x.IdNumber.Contains(IdNumber!), IdNumber is { Length: > 2 });
            pool.Add(x => x.OperatorName.Contains(OperatorName!), OperatorName is { Length: > 2 });
        }
    }
}
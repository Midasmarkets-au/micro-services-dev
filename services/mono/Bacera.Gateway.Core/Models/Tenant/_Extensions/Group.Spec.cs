using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

partial class Group
{
    public sealed class CreateSpec
    {
        public long AccountId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public AccountGroupTypes Type { get; set; }
        public long[] AccountIds { get; set; } = Array.Empty<long>();
    }

    public sealed class UpdateSpec
    {
        public long OwnerAccountId { get; set; }
        public long GroupId { get; set; }

        [Required] public string Name { get; set; } = null!;
    }

    public sealed class UpdateAccountSpec
    {
        public long Id { get; set; }
        public long[] AccountIds { get; set; } = null!;
    }

    public sealed class IdLevelSpec
    {
        public IdLevelSpec(long level, long groupId)
        {
            Level = level;
            GroupId = groupId;
        }

        public long GroupId { get; set; }
        public long Level { get; set; }

        public void Deconstruct(out long level, out long gid)
        {
            level = Level;
            gid = GroupId;
        }
    }
}
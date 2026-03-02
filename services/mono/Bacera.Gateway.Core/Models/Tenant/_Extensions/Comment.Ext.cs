using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway
{
    partial class Comment : IEntity
    {
        public Comment()
        {
            Content = string.Empty;
        }

        public static Comment Build(
            long rowId
            , long operatorPartyIdPartyId
            , CommentTypes type
            , string content
        )
            => new()
            {
                RowId = rowId,
                PartyId = operatorPartyIdPartyId,
                Type = (short)type,
                Content = content,
            };

        [Serializable]
        public class RequestModel
        {
            [Required, Range(1, long.MaxValue)] public long RowId { get; set; }

            [Required] public CommentTypes Type { get; set; }

            [Required] public string Content { get; set; } = null!;
        }


        public sealed class TenantDTO
        {
            public string Content { get; set; } = null!;
            public DateTime CreatedOn { get; set; }
        }

        public sealed class WithCommentSpec
        {
            [Required] public string Comment { get; set; } = null!;
        }
    }
}
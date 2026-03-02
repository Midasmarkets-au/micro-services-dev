using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

using M = Message;

partial class Message
{
    public sealed class CreateSpec
    {
        [Required, Range(1, int.MaxValue)] public long PartyId { get; set; }
        public MessageTypes Type { get; set; } = MessageTypes.System;
        [Required] public string Title { get; set; } = null!;
        [Required] public string Content { get; set; } = null!;

        public long? SenderId { get; set; }
        public MessageSenderTypes SenderType { get; set; } = MessageSenderTypes.System;

        public long? ReferenceId { get; set; }
        public MessageReferenceTypes ReferenceType { get; set; } = MessageReferenceTypes.Unknown;
    }

    public bool IsEmpty() => Id is < 1 or 0;
}
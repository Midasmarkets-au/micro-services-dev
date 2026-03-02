using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

using M = Bacera.Gateway.IpBlackList;

public partial class IpBlackList
{
    public sealed class CreateSpec
    {
        [Required] public string Ip { get; set; } = null!;
        public string Note { get; set; } = "";
        public bool Enabled { get; set; } = true;

        public M ToEntity(string operatorName = "") => new()
        {
            Ip = Ip,
            Note = Note,
            Enabled = true,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            OperatorName = operatorName,
        };
    }

    public sealed class UpdateSpec
    {
        [Required] public string Ip { get; set; } = null!;
        public string Note { get; set; } = "";
        public bool Enabled { get; set; } = true;

        public M ApplyTo(M entity, string operatorName = "")
        {
            entity.Ip = Ip;
            entity.Note = Note;
            entity.Enabled = Enabled;
            entity.UpdatedOn = DateTime.UtcNow;
            entity.OperatorName = operatorName;
            return entity;
        }
    }
}
using System.ComponentModel.DataAnnotations.Schema;

namespace Bacera.Gateway.Auth.Db;

public class IpBlackList
{
    [Column("Id")]
    public long Id { get; set; }

    [Column("Ip")]
    public string Ip { get; set; } = string.Empty;
}

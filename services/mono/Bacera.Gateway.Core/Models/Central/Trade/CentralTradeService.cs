namespace Bacera.Gateway;

public class CentralTradeService
{
    public int Id { get; set; }

    public short Platform { get; set; }

    public int Priority { get; set; }

    public short IsAllowAccountCreation { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public string Configuration { get; set; } = null!;
}
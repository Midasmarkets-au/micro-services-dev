namespace Bacera.Gateway;

public abstract class Auditable : IAuditable
{
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}
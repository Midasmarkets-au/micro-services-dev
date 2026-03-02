namespace Bacera.Gateway;

public interface IHasOptionalEmail
{
    public string? Email { get; }
}
public interface IHasEmail
{
    public string Email { get; }
}
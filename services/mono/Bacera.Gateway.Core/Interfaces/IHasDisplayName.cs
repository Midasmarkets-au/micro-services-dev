namespace Bacera.Gateway;

public interface IHasNativeName
{
    string NativeName { get; set; }
}

public interface IHasLastName
{
    string LastName { get; set; }
}

public interface IHasFirstName
{
    string FirstName { get; set; }
}

public interface IHasDisplayName : IHasFirstName, IHasLastName, IHasNativeName
{
    public string DisplayName { get; }
}
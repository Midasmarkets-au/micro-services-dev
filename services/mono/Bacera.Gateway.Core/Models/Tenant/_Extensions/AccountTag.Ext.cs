namespace Bacera.Gateway;

public partial class AccountTag
{
    public static AccountTag Build(string name) => new()
    {
        Name = name,
    };
}
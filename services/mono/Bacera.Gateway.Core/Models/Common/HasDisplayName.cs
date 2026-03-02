namespace Bacera.Gateway;

public abstract class HasDisplayName : IHasDisplayName
{
    public string NativeName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrEmpty(NativeName))
            {
                return NativeName;
            }

            if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
            {
                return (FirstName + " " + LastName).Trim();
            }

            if (!string.IsNullOrWhiteSpace(FirstName))
                return FirstName;

            if (!string.IsNullOrWhiteSpace(LastName))
                return LastName;

            return string.Empty;
        }
    }
}
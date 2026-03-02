namespace Bacera.Gateway.Web.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Infrastructure)]
[Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
public class LegacyLaravelTests
{
    public LegacyLaravelTests()
    {
    }

    [Fact]
    public void HashTest()
    {
        var hashedPasswords = new List<string>
        {
            "$2y$10$xO7/Jo2zmbcJdA3bdmlCzOc.hduFs/IjE0E.nhYXLRR9tgvmG39Lm",
            "$2y$10$wGFl2ii4qNsfWBIf.oN3ie1pwSDsCXBubaE6tY/FmZez/uWqjSh6u",
        };
        const string plainTextPassword = "Abcd1234";
        
        foreach (var hashedPassword in hashedPasswords)
        {
            var isVerify = BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
            isVerify.ShouldBeTrue();
        }
    }
}
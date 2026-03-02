namespace Bacera.Gateway.Interfaces;

public interface IApplicationTokenService
{
    Task RemoveTokenAsync(ApplicationToken token);
    Task RemoveTokenAsync(long partyId, TokenTypes tokenType, long referenceId);
    Task<bool> VerifyTokenAsync(ApplicationToken token);
    Task<ApplicationToken?> GenerateTokenAsync(ApplicationToken tokenRequest, TimeSpan? expiration = null);
}
using Bacera.Gateway.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Bacera.Gateway.Services;

public class ApplicationTokenService : IApplicationTokenService
{
    private const string TokenProvider = "Email";
    private const string CacheKeyPrefix = "token_";

    private readonly IDistributedCache _cache;
    private readonly UserManager<Auth.User> _userManager;
    private readonly Tenancy _tenancy;

    public ApplicationTokenService(Tenancy tenancy, UserManager<Auth.User> userManager,
        IDistributedCache cache)
    {
        _cache = cache;
        _userManager = userManager;
        _tenancy = tenancy;
    }

    public async Task RemoveTokenAsync(ApplicationToken token)
    {
        await _cache.RemoveAsync(GetCacheKey(token));
    }

    public async Task RemoveTokenAsync(long partyId, TokenTypes tokenType, long referenceId)
    {
        var token = ApplicationToken.Build(partyId, tokenType, referenceId);
        await _cache.RemoveAsync(GetCacheKey(token));
    }

    public async Task<bool> VerifyTokenAsync(ApplicationToken token)
    {
        var tokenString = await _cache.GetStringAsync(GetCacheKey(token));
        if (string.IsNullOrEmpty(tokenString))
            return false;

        if (token.Token != tokenString)
            return false;

        return await ValidateUserToken(token.PartyId, tokenString);
    }

    public async Task<ApplicationToken?> GenerateTokenAsync(ApplicationToken tokenRequest, TimeSpan? expiration)
    {
        expiration ??= TimeSpan.FromDays(1);

        var token = await GenerateUserToken(tokenRequest.PartyId);
        if (string.IsNullOrEmpty(token)) return null;
        tokenRequest.SetToken(token);
        await _cache.SetStringAsync(GetCacheKey(tokenRequest), token,
            new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.UtcNow.Add(expiration.Value) });
        return tokenRequest;
    }

    private long GetTenantId() => _tenancy.GetTenantId();
    private string GetCacheKeyPrefix() => $"{CacheKeyPrefix}_TENANT:{GetTenantId()}_";

    private string GetCacheKey(ApplicationToken token)
        => GetCacheKeyPrefix() + "_TYPE:" + token.ReferenceType + "_ID:" + token.ReferenceId;

    private async Task<string> GenerateUserToken(long partyId)
    {
        await Task.Delay(0);
        return partyId > 0 ? _userManager.GenerateNewAuthenticatorKey() : string.Empty;
    }

    private async Task<bool> ValidateUserToken(long partyId, string token)
    {
        await Task.Delay(0);
        return partyId > 0 && !string.IsNullOrEmpty(token);
    }
}
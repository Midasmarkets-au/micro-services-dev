using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services;

public class CentralService
{
    private readonly CentralDbContext _centralCtx;

    public CentralService(CentralDbContext centralCtx)
    {
        _centralCtx = centralCtx;
    }

    public Task<bool> IsAccountNumberExists(long accountNumber)
        => _centralCtx.CentralAccounts.AnyAsync(x => x.AccountNumber == accountNumber);
}
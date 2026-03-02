using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bacera.Gateway;

public class CentralDbContextFactory : IDesignTimeDbContextFactory<CentralDbContext>
{
    public CentralDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CentralDbContext>();
        return new CentralDbContext(optionsBuilder.Options);
    }
}
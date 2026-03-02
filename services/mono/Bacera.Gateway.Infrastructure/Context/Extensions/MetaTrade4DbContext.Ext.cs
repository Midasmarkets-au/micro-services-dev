using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Integration;

partial class MetaTrade4DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseMySql("server=localhost;port=3306;user=dev;database=bcr_staging;password=dev",
                ServerVersion.Parse("5.7.38-mysql"));
        }
    }
}
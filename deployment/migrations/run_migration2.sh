~/.dotnet/tools/dotnet-ef database update --context AuthDbContext --project services/mono/Bacera.Gateway.Infrastructure --startup-project services/mono/Bacera.Gateway.Web --connection "Host=localhost;Port=5432;Database=portal_central;Username=lucaslee;Password="

~/.dotnet/tools/dotnet-ef database update --context CentralDbContext --project services/mono/Bacera.Gateway.Infrastructure --startup-project services/mono/Bacera.Gateway.Web --connection "Host=localhost;Port=5432;Database=portal_central;Username=lucaslee;Password="

~/.dotnet/tools/dotnet-ef database update --context TenantDbContext --project services/mono/Bacera.Gateway.Infrastructure --startup-project services/mono/Bacera.Gateway.Web --connection "Host=localhost;Port=5432;Database=portal_tenant_au;Username=lucaslee;Password="

~/.dotnet/tools/dotnet-ef database update --context TenantDbContext --project services/mono/Bacera.Gateway.Infrastructure --startup-project services/mono/Bacera.Gateway.Web --connection "Host=localhost;Port=5432;Database=portal_tenant_bvi;Username=lucaslee;Password="

~/.dotnet/tools/dotnet-ef database update --context TenantDbContext --project services/mono/Bacera.Gateway.Infrastructure --startup-project services/mono/Bacera.Gateway.Web --connection "Host=localhost;Port=5432;Database=portal_tenant_sea;Username=lucaslee;Password="

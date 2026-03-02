# Migrations

### Install dotnet ef tools

```
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```  

### Central Database

migrate  
`dotnet ef migrations add InitCentralDatabase -c CentralDbContext -o Data/Migrations/CentralDb`

update database  
`dotnet ef database update -c CentralDbContext --connection "connection string"`

### Identity Database (for both Central & Tenant)

`dotnet ef migrations add InitAuthDatabase -c AuthDbContext -o Data/Migrations/AuthDb`

`dotnet ef database update -c AuthDbContext`

### Tenant Database

Migration

```
dotnet ef migrations add InitTenantDatabase -c TenantDbContext -o Data/Migrations/TenantDb
```

Reset

```
dotnet ef database update 0 -c TenantDbContext --connection  "Host=localhost;Port=5432;Username=postgres;Password=dev;Database=portal_tenant_demo;"
```  

Update  
`dotnet ef database update -c TenantDbContext --connection  "Host=localhost;Port=5432;Username=postgres;Password=dev;Database=portal_tenant_demo;"`

Build Model from Database

```

dotnet ef dbcontext scaffold "Host=localhost;Database=portal_tenant_development;Username=postgres;Password=dev;" Npgsql.EntityFrameworkCore.PostgreSQL -o GeneratedModels --context-dir Context --context TenantDbContext -n Bacera.Gateway -f -v --schema acct --schema trd --schema core --schema sto --schema cms --schema rpt

```

### Build Model from Database

`dotnet ef dbcontext scaffold "Host=localhost;Database=portal_central;Username=postgres;Password=dev;" Npgsql.EntityFrameworkCore.PostgreSQL -o Models --context-dir Context --context TenantDbContext -n Bacera.Gateway -f -v --schema acct --schema trd --schema core`

### MT4 Database scaffold

`dotnet ef dbcontext scaffold "servr=localhost;Database=MT4;Username=dev;Password=dev7!aws;" Npgsql.EntityFrameworkCore.PostgreSQL -o Models --context-dir Context --context TenantDbContext -n Bacera.Gateway -f -v --schema acct --schema trd --schema core`

### MetaTrader Copy Trade SSL configure

`sudo edit etc/ssl/openssl.conf`

* Added to the top  
  `openssl_conf = default_conf`

* added to the end

```
[ default_conf ]
ssl_conf = ssl_sect

[ssl_sect]
system_default = system_default_sect

[system_default_sect]
MinProtocol = TLSv1
CipherString = DEFAULT:@SECLEVEL=0
```

For Ubuntu 18.04  
`CipherString = DEFAULT:@SECLEVEL=1`

Testing by Curl  
`curl --location --request POST 'https://202.66.136.54:8850/authorize' --header 'Content-Type: application/json' --data-raw '{   "login": 1090,  "password": "abc123"}' -k -v`

## Database in Docker

`docker run --name postgres-dev -p 5432:5432 -e POSTGRES_PASSWORD=dev -d postgres`

## MeiliSearch in Docker

`docker run -d --name meilisa-dev -p 7700:7700 -v /home/yinsen/dev/data/meili_data:/meili_data   getmeili/meilisearch:v1.2 meilisearch --master-key="V1loZWw1ZGMT6Q"`

## Redis in Docker

`docker run -d --name redis-dev -p 6379:6379 redis`

## Database change instruction

1. Change database
2. Run `dotnet ef dbcontext scaffold` in command line under folder: `portal\src\Bacera.Gateway.Infrastructure`
3. Manually copy Models cs files from `portal\src\Bacera.Gateway.Infrastructure\GeneratedModels`
   to `portal\src\Bacera.Gateway.Core\Models\Tenant`
4. Manually change files to passed the build
    - Keep "TransferView.cs", "TradeService" unchanged
    - Fix "TenantDbContext"
5. Remove tenant migration
6. Re-generate tenant migration

# IdentityServer4

`dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb`
`dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb`

`dotnet ef database update -c ConfigurationDbContext --connection "Host=localhost;Port=5432;Username=postgres;Password=dev;Database=portal_tenant_demo;"`

`dotnet ef database update -c PersistedGrantDbContext --connection "Host=localhost;Port=5432;Username=postgres;Password=dev;Database=portal_tenant_demo;"`


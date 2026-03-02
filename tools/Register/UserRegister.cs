using Bacera.Gateway;
using Bacera.Gateway.Auth;
using Microsoft.EntityFrameworkCore;

namespace Register;

public class UserRegister
{
    private readonly TenantDbContext _ctx;
    private readonly AuthDbContext _authCtx;

    public UserRegister(TenantDbContext ctx, AuthDbContext authCtx)
    {
        _ctx = ctx;
        _authCtx = authCtx;
    }

    public async Task<Bacera.Gateway.Auth.User> CreateUser(string email, string name)
    {
        var userExists = await _authCtx.Users
            .FirstOrDefaultAsync(x => x.Email != null
                                      && x.Email.ToUpper() == email.Trim().ToUpper());
        if (userExists != null)
        {
            Console.WriteLine($"User already exists: {email}");
            return userExists;
        }

        Console.WriteLine($"Creating new user: {email}");

        Console.WriteLine($"Creating party for: {name}");
        var party = new Party
        {
            Name = name,
            NativeName = name,
            Code = Guid.NewGuid().ToString()[..10],
            Note = "",
            SiteId = 1, // Default site ID
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow
        };
        
        Console.WriteLine($"Party object created, adding to context...");
        await _ctx.Parties.AddAsync(party);
        Console.WriteLine($"Saving party to database...");
        await _ctx.SaveChangesAsync();
        Console.WriteLine($"Party saved with ID: {party.Id}");

        var nameSplit = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Console.WriteLine($"Creating user object for email: {email.Trim().ToLower()}");
        var user = Bacera.Gateway.Auth.User.Create(email.Trim().ToLower());
        Console.WriteLine($"User object created: {user != null}");
        if (user == null)
        {
            throw new InvalidOperationException("User.Create returned null");
        }
        Console.WriteLine($"Setting user properties...");
        Console.WriteLine($"User.UserName: '{user.UserName}', User.Email: '{user.Email}'");
        
        // Set UserName if it's null
        if (string.IsNullOrEmpty(user.UserName))
        {
            user.UserName = email.Trim().ToLower();
        }
        
        user.NormalizedUserName = user.UserName!.ToUpper();
        user.NormalizedEmail = user.Email!.ToUpper();
        user.NativeName = name;
        user.FirstName = nameSplit.Length > 0 ? nameSplit[0] : name;
        user.LastName = nameSplit.Length > 1 ? nameSplit[1] : "";
        user.PartyId = party.Id;
        user.Uid = Utils.GenerateUniqueId();
        user.EmailConfirmed = true;
        user.SecurityStamp = Guid.NewGuid().ToString();

        // Set a default password (you should change this!)
        var defaultPassword = "Admin@123456";
        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Bacera.Gateway.Auth.User>();
        user.PasswordHash = passwordHasher.HashPassword(user, defaultPassword);
        
        Console.WriteLine($"User created with default password: {defaultPassword}");

        await _authCtx.Users.AddAsync(user);
        await _authCtx.SaveChangesAsync();

        var wallet = Wallet.Build(party.Id, CurrencyTypes.USD);
        await _ctx.Wallets.AddAsync(wallet);
        await _ctx.SaveChangesAsync();

        return user;
    }

    public async Task<Account> CreateAccount(Bacera.Gateway.Auth.User user, string code)
    {
        var accountExists = _ctx.Accounts
            .Where(x => x.Role == (int)AccountRoleTypes.Sales)
            .FirstOrDefault(x => x.PartyId == user.PartyId);
        if (accountExists != null)
            return accountExists;

        var account = new Account
        {
            Name = user.NativeName,
            PartyId = user.PartyId,
            Uid = Utils.GenerateUniqueId(),
            CurrencyId = (int)CurrencyTypes.USD,
            Type = (int)AccountTypes.Standard,
            Role = (int)AccountRoleTypes.Sales,
            Code = code,
            ReferPath = "",
            FundType = 1,
            SearchText = $"{user.NativeName} {user.Email} {code}",
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };
        await _ctx.Accounts.AddAsync(account);
        await _ctx.SaveChangesAsync();
        return account;
    }

    public async Task<Group> CreateGroup(Account account)
    {
        var exists = await _ctx.Groups.FirstOrDefaultAsync(x => x.Name == account.Code);
        if (exists != null)
            return exists;

        var group = Group.CreateSalesGroup(account.Id, account.Code);
        await _ctx.Groups.AddAsync(group);
        await _ctx.SaveChangesAsync();
        return group;
    }

    public async Task<bool> CreateUserClaims(User user, Account account)
    {
        if (!await _authCtx.UserClaims.AnyAsync(x =>
                x.UserId == user.Id && x.ClaimType == UserClaimTypes.SalesAccount &&
                x.ClaimValue == account.Uid.ToString()))
        {
            var claim = new UserClaim
            {
                UserId = user.Id,
                ClaimType = UserClaimTypes.SalesAccount,
                ClaimValue = account.Uid.ToString()
            };
            await _authCtx.UserClaims.AddAsync(claim);
        }

        if (!await _authCtx.UserClaims.AnyAsync(x =>
                x.UserId == user.Id && x.ClaimType == UserClaimTypes.PartyId &&
                x.ClaimValue == user.PartyId.ToString()))
        {
            var claim = new UserClaim
            {
                UserId = user.Id,
                ClaimType = UserClaimTypes.PartyId,
                ClaimValue = user.PartyId.ToString()
            };
            await _authCtx.UserClaims.AddAsync(claim);
        }

        if (!await _authCtx.UserClaims.AnyAsync(x =>
                x.UserId == user.Id && x.ClaimType == UserClaimTypes.TenantId &&
                x.ClaimValue == "10000"))
        {
            var claim = new UserClaim
            {
                UserId = user.Id,
                ClaimType = UserClaimTypes.TenantId,
                ClaimValue = "10000"
            };
            await _authCtx.UserClaims.AddAsync(claim);
        }

        await _authCtx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateReferralCode(Account account)
    {
        if (!await _ctx.ReferralCodes.AnyAsync(x =>
                x.PartyId == account.PartyId && x.AccountId == account.Id &&
                x.ServiceType == (int)ReferralServiceTypes.Agent)
            && !await _ctx.ReferralCodes.AnyAsync(x =>
                x.Code == account.Code + "_IB"))
        {
            var code = new ReferralCode
            {
                PartyId = account.PartyId,
                AccountId = account.Id,
                Code = account.Code + "_IB",
                ServiceType = (int)ReferralServiceTypes.Agent,
                Name = account.Code + "_IB",
            };
            await _ctx.ReferralCodes.AddAsync(code);
        }

        if (!await _ctx.ReferralCodes.AnyAsync(x =>
                x.PartyId == account.PartyId && x.AccountId == account.Id &&
                x.ServiceType == (int)ReferralServiceTypes.Client)
            && !await _ctx.ReferralCodes.AnyAsync(x =>
                x.Code == account.Code + "_CL")
           )
        {
            var code = new ReferralCode
            {
                PartyId = account.PartyId,
                AccountId = account.Id,
                Code = account.Code + "_CL",
                ServiceType = (int)ReferralServiceTypes.Client,
                Name = account.Code + "_CL",
            };
            await _ctx.ReferralCodes.AddAsync(code);
        }

        await _ctx.SaveChangesAsync();
        return true;
    }

    public async Task AssignUserRole(Bacera.Gateway.Auth.User user)
    {
        var salesRole = new Bacera.Gateway.Auth.UserRole
        {
            UserId = user.Id,
            RoleId = (int)UserRoleTypes.Sales
        };
        var clientRole = new Bacera.Gateway.Auth.UserRole
        {
            UserId = user.Id,
            RoleId = (int)UserRoleTypes.Client
        };

        if (!await _authCtx.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == (int)UserRoleTypes.Sales))
            await _authCtx.UserRoles.AddAsync(salesRole);

        if (!await _authCtx.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == (int)UserRoleTypes.Client))
            await _authCtx.UserRoles.AddAsync(clientRole);

        await _authCtx.SaveChangesAsync();
    }

    // public async Task<bool> UserEmailExists(string email)
    // {
    //     return await _authCtx.Users.AnyAsync(x => x.Email == email);
    // }
    //
    // public async Task<bool> DeleteUser(string email)
    // {
    //     var user = await _authCtx.Users.Where(x => x.Email.ToUpper().Trim() == email.Trim().ToUpper()).ToListAsync();
    //     var partyIds = user.Select(x => x.PartyId).ToList();
    //     await DeletePartyId(partyIds);
    //     _authCtx.Users.RemoveRange(user);
    //     await _authCtx.SaveChangesAsync();
    //     return true;
    // }
    //
    // public async Task<bool> DeletePartyId(List<long> partyIds)
    // {
    //     var accounts = await _ctx.Accounts.Where(x => partyIds.Contains(x.PartyId)).ToListAsync();
    //     _ctx.Accounts.RemoveRange(accounts);
    //     await _ctx.SaveChangesAsync();
    //
    //     var refCodes = await _ctx.ReferralCodes.Where(x => partyIds.Contains(x.PartyId)).ToListAsync();
    //     _ctx.ReferralCodes.RemoveRange(refCodes);
    //     await _ctx.SaveChangesAsync();
    //     var parties = await _ctx.Parties.Where(x => partyIds.Contains(x.Id)).ToListAsync();
    //     _ctx.Parties.RemoveRange(parties);
    //     await _ctx.SaveChangesAsync();
    //     return true;
    // }


    public static List<ImportUser> ReadUserFile(string file)
    {
        var csv = File.ReadAllLines(file);
        var users = new List<ImportUser>();
        foreach (var line in csv.Skip(1))
        {
            var data = line.Split(',');
            var user = new ImportUser
            {
                AccountNumber = long.Parse(data[0]),
                Code = data[2],
                Name = data[4],
                Email = data[6],
            };
            users.Add(user);
        }

        return users;
    }

    public class ImportUser
    {
        public long AccountNumber { get; set; }
        public string Code { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
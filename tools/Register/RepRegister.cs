using Bacera.Gateway;
using Bacera.Gateway.Auth;
using Microsoft.EntityFrameworkCore;

namespace Register
{
    public class RepRegister
    {
        private readonly TenantDbContext _ctx;
        private readonly AuthDbContext _authCtx;

        public RepRegister(TenantDbContext ctx, AuthDbContext authCtx)
        {
            _ctx = ctx;
            _authCtx = authCtx;
        }

        public async Task<Bacera.Gateway.Auth.User> CreateUser(string email, string name)
        {
            var userExists = await _authCtx.Users
                .FirstOrDefaultAsync(x => x.Email != null && x.Email.ToUpper() == email.Trim().ToUpper());
            if (userExists != null)
            {
                Console.WriteLine($"User already exists: {email}");
                return userExists;
            }

            Console.WriteLine($"Creating new user: {email}");
            var party = new Party
            {
                Name = name,
                NativeName = name,
                Code = Guid.NewGuid().ToString()[..10],
                Note = "",
                SiteId = 1,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };
            await _ctx.Parties.AddAsync(party);
            await _ctx.SaveChangesAsync();

            var nameSplit = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var user = Bacera.Gateway.Auth.User.Create(email.Trim().ToLower());
            if (user == null)
                throw new InvalidOperationException("User.Create returned null");
            user.UserName = string.IsNullOrEmpty(user.UserName) ? email.Trim().ToLower() : user.UserName;
            user.NormalizedUserName = user.UserName!.ToUpper();
            user.NormalizedEmail = user.Email!.ToUpper();
            user.NativeName = name;
            user.FirstName = nameSplit.Length > 0 ? nameSplit[0] : name;
            user.LastName = nameSplit.Length > 1 ? nameSplit[1] : "";
            user.PartyId = party.Id;
            user.Uid = Utils.GenerateUniqueId();
            user.EmailConfirmed = true;
            user.SecurityStamp = Guid.NewGuid().ToString();
            var defaultPassword = "Admin@123456";
            var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Bacera.Gateway.Auth.User>();
            user.PasswordHash = passwordHasher.HashPassword(user, defaultPassword);
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
                .Where(x => x.Role == (int)AccountRoleTypes.Rep)
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
                Role = (int)AccountRoleTypes.Rep,
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
            var group = Group.CreateSalesGroup(account.Id, account.Code); // If reps need a different group, adjust here
            await _ctx.Groups.AddAsync(group);
            await _ctx.SaveChangesAsync();
            return group;
        }

        public async Task<bool> CreateUserClaims(User user, Account account)
        {
            if (!await _authCtx.UserClaims.AnyAsync(x =>
                    x.UserId == user.Id && x.ClaimType == UserClaimTypes.RepAccount &&
                    x.ClaimValue == account.Uid.ToString()))
            {
                var claim = new UserClaim
                {
                    UserId = user.Id,
                    ClaimType = UserClaimTypes.RepAccount,
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
                    x.Code == account.Code + "_REP"))
            {
                var code = new ReferralCode
                {
                    PartyId = account.PartyId,
                    AccountId = account.Id,
                    Code = account.Code + "_REP",
                    ServiceType = (int)ReferralServiceTypes.Agent,
                    Name = account.Code + "_REP",
                };
                await _ctx.ReferralCodes.AddAsync(code);
            }
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task AssignUserRole(Bacera.Gateway.Auth.User user)
        {
            var repsRole = new Bacera.Gateway.Auth.UserRole
            {
                UserId = user.Id,
                RoleId = (int)UserRoleTypes.Rep
            };
            var clientRole = new Bacera.Gateway.Auth.UserRole
            {
                UserId = user.Id,
                RoleId = (int)UserRoleTypes.Client
            };
            if (!await _authCtx.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == (int)UserRoleTypes.Rep))
                await _authCtx.UserRoles.AddAsync(repsRole);
            if (!await _authCtx.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == (int)UserRoleTypes.Client))
                await _authCtx.UserRoles.AddAsync(clientRole);
            await _authCtx.SaveChangesAsync();
        }

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
}
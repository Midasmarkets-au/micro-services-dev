using System.Text;
using Bacera.Gateway;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Register;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true);
        var configuration = builder.Build();
        var tenantConn = configuration.GetConnectionString("DefaultConnection");
        var centralConn = configuration.GetConnectionString("CentralConnection");
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseNpgsql(tenantConn)
            .Options;
        var authOptions = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(centralConn)
            .Options;
        var dbContext = new TenantDbContext(options);
        var authDbContext = new AuthDbContext(authOptions);

        await RegisterByCsvFile(dbContext, authDbContext);
        
        // Update passwords for existing users without passwords
        // await UpdatePasswordsForExistingUsers(authDbContext);
        
        // await sendResetPasswordEmail(authDbContext);
        Console.WriteLine($"Done!");
    }

    private static async Task SendResetPasswordEmail(AuthDbContext authDbContext)
    {
        var url = "https://portal.thebcr.com/reset-password";
        var rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        var listPath = Path.Combine(rootPath, "Data/user_list.csv");
        var userList = RepRegister.ReadUserFile(listPath);
        // var client = new HttpClient
        // {
        //     BaseAddress = new Uri("https://api.bvi.thebcr.com")
        // };

        var emailList = userList.Select(x => x.Email.Trim()).Distinct().ToList();
        foreach (var email in emailList)
            // foreach (var email in new string[] { "feng@bacera.com", "feng+dev@bacera.com" })
        {
            var request = new Dictionary<string, string>
            {
                { "email", email },
                { "ResetUrl", url }
            };
            var content = new StringContent(Utils.JsonSerializeObject(request), Encoding.UTF8, "application/json");
            // var result = await client.PostAsync("/api/v1/user/password/forgot", content);
            // Console.WriteLine($"[{email}] {result.StatusCode}");
        }
    }

    private static async Task RegisterByCsvFile(TenantDbContext dbContext, AuthDbContext authDbContext)
    {
        var rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        var listPath = Path.Combine(rootPath, "Data/user_list.csv");

        var userList = RepRegister.ReadUserFile(listPath);

        var registerSvc = new RepRegister(dbContext, authDbContext);

        var i = 1;
        foreach (var importUser in userList)
        {
            // await registerSvc.DeleteUser(importUser.Email);
            var user = await registerSvc.CreateUser(importUser.Email, importUser.Name);
            var account = await registerSvc.CreateAccount(user, importUser.Code);
            var group = await registerSvc.CreateGroup(account);
            await registerSvc.AssignUserRole(user);
            await registerSvc.CreateUserClaims(user, account);
            await registerSvc.CreateReferralCode(account);
            Console.WriteLine($"[{i}] {user.Email} U: {user.Id} A: {account.Id} G: {group.Id}");
            i++;
        }
    }

    // private static async Task UpdatePasswordsForExistingUsers(AuthDbContext authDbContext)
    // {
    //     Console.WriteLine("Checking for users without passwords...");
        
    //     var existingEmails = new List<string>
    //     {
    //         "sarah.johnson@broker.com",
    //         "mike.wilson@trading.com", 
    //         "lisa.davis@finance.com",
    //         "robert.brown@investment.com"
    //     };
        
    //     var passwordUpdater = new PasswordUpdater(authDbContext);
    //     await passwordUpdater.UpdatePasswordsForUsers(existingEmails);
    // }
}
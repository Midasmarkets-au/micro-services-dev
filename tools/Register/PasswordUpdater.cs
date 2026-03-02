using Bacera.Gateway;
using Microsoft.EntityFrameworkCore;

namespace Register;

public class PasswordUpdater
{
    private readonly AuthDbContext _authCtx;

    public PasswordUpdater(AuthDbContext authCtx)
    {
        _authCtx = authCtx;
    }

    public async Task UpdatePasswordsForUsers(List<string> emails, string password = "Admin@123456")
    {
        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Bacera.Gateway.Auth.User>();
        
        foreach (var email in emails)
        {
            var user = await _authCtx.Users
                .FirstOrDefaultAsync(x => x.Email != null && x.Email.ToUpper() == email.Trim().ToUpper());
                
            if (user != null)
            {
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    user.PasswordHash = passwordHasher.HashPassword(user, password);
                    user.SecurityStamp = Guid.NewGuid().ToString();
                    Console.WriteLine($"Password set for user: {email}");
                }
                else
                {
                    Console.WriteLine($"User {email} already has a password - skipping");
                }
            }
            else
            {
                Console.WriteLine($"User not found: {email}");
            }
        }
        
        await _authCtx.SaveChangesAsync();
        Console.WriteLine("Password update completed!");
    }
} 
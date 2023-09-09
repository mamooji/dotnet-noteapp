using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence.Seeders;

public class DefaultUserSeeder
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DefaultUserSeeder(IApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task Seed()
    {
        await SeedUser("admin@test.com", "admin", "password", CustomRole.Admin, "Admin", "LastName");
        await SeedUser("user@test.com", "user", "password", CustomRole.User, "User", "LastName");
    }

    /// <summary>
    ///     Attempts to seed a user. If a user with the same username exists, returns existing user id.
    /// </summary>
    /// <returns>New or existing user id</returns>
    public async Task<string> SeedUser(
        string email,
        string userName,
        string password,
        string role = null,
        string firstName = null,
        string lastName = null,
        string phoneNumber = null
    )
    {
        var existingUser = _userManager.Users.FirstOrDefault(u => u.UserName == userName);
        if (existingUser != null)
        {
            if (role != null) await _userManager.AddToRoleAsync(existingUser, role);
            return existingUser.Id;
        }

        var identityUser = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber
        };

        await _userManager.CreateAsync(identityUser, password);
        await _context.SaveChangesAsync();

        if (role != null) await _userManager.AddToRoleAsync(identityUser, role);

        return identityUser.Id;
    }
}
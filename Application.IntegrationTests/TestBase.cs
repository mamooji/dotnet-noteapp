using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Identity;
using NUnit.Framework;

namespace Backend.Application.IntegrationTests;

using static Testing;

public class TestBase
{
    public virtual bool ResetStateDuringTestSetup => true;

    [SetUp]
    public async Task TestSetUp()
    {
        if (ResetStateDuringTestSetup) await ResetState();
    }


    protected async Task<string> SeedRole(string roleName, int? projectId = null)
    {
        var identityRole = new IdentityRole(roleName)
        {
            Name = roleName
        };

        var roleManager = GetRoleManager();

        await roleManager.CreateAsync(identityRole);
        return identityRole.Id;
    }


// protected async Task SeedClaim(string roleId, AuthorizationResource resource, AuthorizationVerb verb, string type = null)
// {
//     await SeedClaim(roleId, new QuesClaim(resource, verb), type);
// }

    protected async Task SeedClaim(string roleId, string claim, string type = null)
    {
        var roleManager = GetRoleManager();

        var identityRole = await roleManager.FindByIdAsync(roleId);

        await roleManager.AddClaimAsync(identityRole, new Claim(type ?? CustomClaimType.Permission, claim));
    }

    protected async Task SeedClaimForUser(string userId, Claim claim)
    {
        var userManager = GetUserManager();

        var user = await userManager.FindByIdAsync(userId);

        await userManager.AddClaimAsync(user, claim);
    }


    protected async Task<string> SeedUser(
        string email,
        string userName,
        string firstName = null,
        string lastName = null,
        DateTimeOffset? lockEndDate = null
    )
    {
        var context = GetContext();

        var user = new ApplicationUser
        {
            Email = email,
            UserName = userName,
            FirstName = firstName,
            LastName = lastName
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        await context.SaveChangesAsync();

        return user.Id;
    }
}
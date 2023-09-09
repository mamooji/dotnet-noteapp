using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly IApplicationDbContext _context;
    private readonly Microsoft.AspNetCore.Identity.RoleManager<IdentityRole> _roleManager;

    public RoleService(Microsoft.AspNetCore.Identity.RoleManager<IdentityRole> roleManager,
        IApplicationDbContext context
    )
    {
        _roleManager = roleManager;
        _context = context;
    }

    /// <summary>
    ///     Adds the claim to role.
    /// </summary>
    /// <param name="claim">The add claim.</param>
    /// <param name="role">Name of the role.</param>
    public async Task AddClaimToRole(string claim, string role)
    {
        var identityRole = await _roleManager.FindByNameAsync(role);
        await _roleManager.AddClaimAsync(identityRole, new Claim(CustomClaimType.Permission, claim));
    }

    /// <summary>
    ///     Adds multiple claims to role.
    /// </summary>
    /// <param name="claims">The claims being added.</param>
    /// <param name="role">Name of the role.</param>
    public async Task AddClaimsToRole(IEnumerable<string> claims, string role)
    {
        var identityRole = await _roleManager.FindByNameAsync(role);

        foreach (var claim in claims)
            await _roleManager.AddClaimAsync(identityRole, new Claim(CustomClaimType.Permission, claim));
    }

    /// <summary>
    ///     Deletes the specified role name.
    /// </summary>
    /// <param name="role">Name of the role.</param>
    public async Task Delete(string role)
    {
        var identityRole = await _roleManager.FindByNameAsync(role);

        if (identityRole == null) throw new NotFoundException("Cannot not find specified role name.");

        await _roleManager.DeleteAsync(identityRole);
    }

    /// <summary>
    ///     Deletes the claim from role.
    /// </summary>
    /// <param name="claim">The delete claim.</param>
    /// <param name="role">Name of the role.</param>
    public async Task DeleteClaimFromRole(string claim, string role)
    {
        var identityRole = await _roleManager.FindByNameAsync(role);

        await _roleManager.RemoveClaimAsync(identityRole, new Claim(CustomClaimType.Permission, claim));
    }

    /// <summary>
    ///     Deletes multiple claims from role.
    /// </summary>
    /// <param name="claims">The claims being deleted.</param>
    /// <param name="role">Name of the role.</param>
    public async Task DeleteClaimsFromRole(IEnumerable<string> claims, string role)
    {
        var identityRole = await _roleManager.FindByNameAsync(role);

        foreach (var claim in claims)
            await _roleManager.RemoveClaimAsync(identityRole, new Claim(CustomClaimType.Permission, claim));
    }

    /// <summary>
    ///     Does the claim exist on a role, regardless of project.
    /// </summary>
    /// <param name="claim">Name of the claim.</param>
    /// <param name="role">Name of the role.</param>
    /// <returns></returns>
    public async Task<bool> DoesClaimExist(string claim, string role)
    {
        var identityRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role);

        if (identityRole == null) return false;

        var claims = await _roleManager.GetClaimsAsync(identityRole);

        return claims.Any(c => c.Value.Equals(claim));
    }


    /// <summary>
    ///     Does the role exist.
    /// </summary>
    /// <param name="role">Name of the role.</param>
    /// <returns></returns>
    public async Task<bool> DoesRoleExist(string role)
    {
        return await _roleManager.RoleExistsAsync(role);
    }

    public async Task<List<Claim>> GetClaimsForRole(string role)
    {
        var identityRole = await _roleManager.FindByNameAsync(role);

        if (identityRole == null) return new List<Claim>();

        var claims = await _roleManager.GetClaimsAsync(identityRole);

        return (List<Claim>)claims;
    }

    /// <summary>
    ///     Gets the name of the role identifier from.
    /// </summary>
    /// <param name="role">Name of the role.</param>
    /// <returns></returns>
    public async Task<string> GetRoleIdFromName(string role)
    {
        var identityRole = await _roleManager.FindByNameAsync(role);

        return identityRole.Id;
    }

    /// <summary>
    ///     Gets all roles claims.
    /// </summary>
    /// <returns></returns>
    public Task<List<IdentityRole>> GetAllRoles()
    {
        return _roleManager.Roles.ToListAsync();
    }

    /// <summary>
    ///     Creates the specified role name.
    /// </summary>
    /// <param name="claims">List of claims to add.</param>
    /// <param name="roleName">Name of the role.</param>
    /// <param name="projectId"></param>
    public async Task Create(IEnumerable<string> claims, string roleName)
    {
        var complexRoleName = roleName;

        var identityRole = await _context.Roles
            .Where(r => r.Name == complexRoleName)
            .FirstOrDefaultAsync();

        if (identityRole == null)
        {
            identityRole = new IdentityRole(complexRoleName)
            {
                Name = complexRoleName
            };
            var result = await _roleManager.CreateAsync(identityRole);
            if (!result.Succeeded)
                throw new UnprocessableEntityException(
                    $"Failed to create role {roleName}: {result.Errors.First().Description}");
        }

        var existingClaims = await _roleManager.GetClaimsAsync(identityRole);
        var existingClaimValues = existingClaims.ToList().Select(x => x.Value).ToArray();

        foreach (var claim in claims)
        {
            var alreadyExists = existingClaimValues.Contains(claim);

            if (alreadyExists) continue;

            await _roleManager.AddClaimAsync(identityRole, new Claim(CustomClaimType.Permission, claim));
        }
    }
}
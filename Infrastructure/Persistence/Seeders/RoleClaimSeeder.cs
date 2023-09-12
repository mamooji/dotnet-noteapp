using Application.Common.Interfaces;
using Domain.Common;
using Domain.ValueObjects;
using QuesBackend.Domain.Common;

namespace Infrastructure.Persistence.Seeders;

public class RoleClaimSeeder
{
    public static List<MyClaim> DefaultUserRoleClaims = new()
    {
        new MyClaim(AuthorizationResource.Asset, AuthorizationVerb.Get),
        new MyClaim(AuthorizationResource.User, AuthorizationVerb.Get)
    };

    private readonly IApplicationDbContext _context;

    private readonly Array _resources = Enum.GetValues(typeof(AuthorizationResource));
    private readonly IRoleService _roleService;

    private readonly Array _verbs = Enum.GetValues(typeof(AuthorizationVerb));

    public RoleClaimSeeder(IApplicationDbContext context, IRoleService roleService)
    {
        _context = context;
        _roleService = roleService;
    }

    /**
         * Seed all of our default roles
         */
    public async Task SeedAllRoleClaims()
    {
        await SeedAdminRoleClaims();
        await SeedUserRoleClaims();
    }

    /**
     * Seed the default role claims for an admin.
     * An admin has access to everything, so we iterate over all resources and verbs, creating identity_role_claim
     * records for each permutation.
     */
    public async Task SeedAdminRoleClaims()
    {
        var role = CustomRole.Admin;
        await DeleteAllClaimsFromRole(role);

        var claims = (from AuthorizationResource resource in _resources
            from AuthorizationVerb verb in _verbs
            select new MyClaim(resource, verb)
            into claim
            select claim.ToString()).ToList();

        await _roleService.Create(claims, role);
    }

    private async Task SeedUserRoleClaims()
    {
        var role = CustomRole.User;
        await DeleteAllClaimsFromRole(role);

        var claimStrings = DefaultUserRoleClaims.Select(c => c.ToString());
        await _roleService.Create(claimStrings, role);
    }

    private async Task DeleteAllClaimsFromRole(string role)
    {
        var roleExists = await _roleService.DoesRoleExist(role);
        if (roleExists)
        {
            var allClaims = await _roleService.GetClaimsForRole(role);
            foreach (var claim in allClaims) await _roleService.DeleteClaimFromRole(claim.Value, role);
        }
    }
}
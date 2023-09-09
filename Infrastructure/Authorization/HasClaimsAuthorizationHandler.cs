using System.Security.Claims;
using Application.Common.Authorization;
using Application.Common.Behaviours;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization;

public class HasClaimsAuthorizationHandler : AuthorizationHandler<HasClaim, HasClaimResource>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoleService _roleService;

    public HasClaimsAuthorizationHandler(ICurrentUserService currentUserService, IRoleService roleService)
    {
        _currentUserService = currentUserService;
        _roleService = roleService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasClaim requirement,
        HasClaimResource resource)
    {
        var user = _currentUserService.ApplicationUser;
        var claimRequired = resource.ClaimRequired;

        if (user == null)
        {
            context.Fail();
            return;
        }

        var userRoles = user.Claims
            .Where(x => x.Type == ClaimTypes.Role)
            .Select(c => c.Value);

        if (userRoles == null || userRoles.Count() == 0)
        {
            // Right now, if someone doesn't have a role, we will fail
            // In the future, we will use the claims via a role OR the one-off claims for an identity
            context.Fail();
            return;
        }

        var hasClaim = false;

        if (!hasClaim)
        {
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}
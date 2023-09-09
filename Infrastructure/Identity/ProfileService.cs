using System.Security.Claims;
using Application.Common.Interfaces;
using Domain.Entities;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

public class ProfileService : IProfileService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<IProfileService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileService(UserManager<ApplicationUser> userManager, IApplicationDbContext context,
        ILogger<ProfileService> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var applicationUser = await _userManager.GetUserAsync(context.Subject);

        if (applicationUser == null)
        {
            _logger.LogWarning("applicationUser not found in ProfileService.GetProfileDataAsync");
            return;
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == applicationUser.Id);

        if (user == null)
        {
            _logger.LogWarning(
                $"user not found in ProfileService.GetProfileDataAsync with ApplicationUserId={applicationUser.Id}");
            return;
        }

        var userId = user.Id;
        var roles = await _userManager.GetRolesAsync(applicationUser);

        var roleClaims = new List<Claim>();
        foreach (var role in roles) roleClaims.Add(new Claim(JwtClaimTypes.Role, role));

        context.IssuedClaims.AddRange(roleClaims);
        context.IssuedClaims.Add(new Claim(CustomClaimType.CanonicalUserId, userId));
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        return Task.CompletedTask;
    }
}
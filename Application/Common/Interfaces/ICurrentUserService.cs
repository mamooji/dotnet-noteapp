using System.Security.Claims;

namespace Application.Common.Interfaces;

public interface ICurrentUserService
{
    string ApplicationUserId { get; }

    ClaimsPrincipal ApplicationUser { get; }

    string AuthToken { get; }
}
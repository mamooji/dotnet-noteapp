using System.Security.Claims;
using Application.Common.Interfaces;

namespace Backend.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        ApplicationUserId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        ApplicationUser = httpContextAccessor.HttpContext?.User;
        AuthToken = GetAuthToken(httpContextAccessor.HttpContext);
    }

    public string ApplicationUserId { get; }

    public ClaimsPrincipal ApplicationUser { get; }

    public string AuthToken { get; }

    private string GetAuthToken(HttpContext httpContext)
    {
        if (httpContext == null) return null;

        if (httpContext.Request.Headers.TryGetValue("Authorization", out var headerAuth))
        {
            var authHeaderValue = headerAuth.First();
            if (!authHeaderValue.StartsWith("Bearer ")) return null;

            var jwtToken = authHeaderValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1];
            return jwtToken;
        }

        return null;
    }
}
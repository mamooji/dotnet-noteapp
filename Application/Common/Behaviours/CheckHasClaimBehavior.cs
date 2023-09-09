using System.Security.Claims;
using Application.Common.Authorization;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Application.Common.Behaviours;

public class HasClaimResource
{
    public string ClaimRequired { get; set; }
}

public class CheckHasClaimBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public CheckHasClaimBehaviour(IAuthorizationService authorizationService,
        ICurrentUserService currentUserService, IApplicationDbContext context, IUserService userService)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _context = context;
        _userService = userService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType();

        var claimIsRequired = Attribute.IsDefined(requestType, typeof(RequireClaimAttribute));

        if (claimIsRequired == false) return await next();

        // If we want to separate 401 from 403 later, we can extract this
        var currentUserId = _currentUserService.ApplicationUserId;

        if (string.IsNullOrEmpty(currentUserId)) throw new UnauthorizedException();

        var requireClaimAttribute =
            (RequireClaimAttribute)Attribute.GetCustomAttribute(requestType, typeof(RequireClaimAttribute));

        if (requireClaimAttribute == null)
            throw new Exception($"Claim is required but attribute is not set for requestType={requestType}");

        var claimRequired = requireClaimAttribute.Claim;

        if (claimRequired.Resource != null)
        {
            var resource = new HasClaimResource { ClaimRequired = claimRequired };

            var authorizationResult =
                await _authorizationService.AuthorizeAsync(ClaimsPrincipal.Current, resource, nameof(HasClaim));

            if (!authorizationResult.Succeeded) throw new ForbiddenException(claimRequired.Resource.ToString());
        }

        return await next();
    }
}
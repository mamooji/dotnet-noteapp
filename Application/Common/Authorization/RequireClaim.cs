using Domain.Common;
using Domain.ValueObjects;
using QuesBackend.Domain.Common;

namespace Application.Common.Authorization;

[AttributeUsage(AttributeTargets.Class |
                AttributeTargets.Struct)
]
public class RequireClaimAttribute : Attribute
{
    public RequireClaimAttribute(MyClaim claim)
    {
        Claim = claim;
    }

    public RequireClaimAttribute(AuthorizationResource resource, AuthorizationVerb verb)
    {
        Claim = new MyClaim(resource, verb);
    }

    public MyClaim Claim { get; }
}
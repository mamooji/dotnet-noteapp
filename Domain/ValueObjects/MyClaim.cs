using Domain.Common;
using QuesBackend.Domain.Common;

namespace Domain.ValueObjects;

/**
     * Shape of a "QuesClaim" string is resource.verb, e.g. "asset.create"
     */
public class MyClaim : ValueObject
{
    private MyClaim()
    {
    }

    public MyClaim(AuthorizationResource resource, AuthorizationVerb verb)
    {
        Resource = resource;
        Verb = verb;
    }

    public AuthorizationResource Resource { get; }

    public AuthorizationVerb Verb { get; }

    public static implicit operator string(MyClaim myClaim)
    {
        return myClaim.ToString();
    }

    public override string ToString()
    {
        return $"{Resource.ToString().ToLower()}.{Verb.ToString().ToLower()}";
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Resource;
        yield return Verb;
    }
}
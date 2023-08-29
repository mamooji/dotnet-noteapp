namespace QuesBackend.Domain.Common;

public enum AuthorizationResource
{
    Claim
}

/// <summary>
///     Returns a list of AuthorizationResource items from array.
/// </summary>
/// <remarks>
///     List is required for foreach loops.
/// </remarks>
public static class AuthorizationResources
{
    public static IEnumerable<AuthorizationResource> GetList()
    {
        return Enum.GetValues(typeof(AuthorizationResource)).OfType<AuthorizationResource>();
    }
}
namespace Domain.Common;

public enum AuthorizationVerb
{
    Create,
    Update,
    Delete,
    Get
}

/// <summary>
///     Returns a list of AuthorizationVerb items from array.
/// </summary>
/// <remarks>
///     List is required for foreach loops.
/// </remarks>
public static class AuthorizationVerbs
{
    public static IEnumerable<AuthorizationVerb> GetList()
    {
        return Enum.GetValues(typeof(AuthorizationVerb)).OfType<AuthorizationVerb>();
    }
}
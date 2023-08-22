namespace Application.Common.Exceptions;

public class FeatureException : Exception
{
    public FeatureException()
        : base("User does not have access to this feature for this project.")
    {
    }
}
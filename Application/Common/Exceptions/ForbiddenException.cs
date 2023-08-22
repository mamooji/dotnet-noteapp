namespace Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string feature)
        : base($"User does not have access to feature {feature}.")
    {
    }

    public ForbiddenException() : this(string.Empty)
    {
    }
}
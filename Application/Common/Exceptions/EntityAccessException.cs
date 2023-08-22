namespace Application.Common.Exceptions;

public class EntityAccessException : Exception
{
    public EntityAccessException()
    {
    }

    public EntityAccessException(string message)
        : base(message)
    {
    }

    public EntityAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
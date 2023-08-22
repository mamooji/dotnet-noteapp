namespace Application.Common.Exceptions;

public class LogicalException : Exception
{
    public LogicalException()
    {
    }

    public LogicalException(string message)
        : base(message)
    {
    }

    public LogicalException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public LogicalException(string name, object key)
        : base($"Entity \"{name}\" ({key}) had something unexpected go wrong")
    {
    }
}
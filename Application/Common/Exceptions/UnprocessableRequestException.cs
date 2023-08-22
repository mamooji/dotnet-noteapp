namespace Application.Common.Exceptions;

public class UnprocessableEntityException : Exception
{
    public UnprocessableEntityException()
    {
    }

    public UnprocessableEntityException(string message)
        : base(message)
    {
    }

    public UnprocessableEntityException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public UnprocessableEntityException(string name, object key)
        : base($"Entity \"{name}\" ({key}) had something unexpected go wrong")
    {
    }
}
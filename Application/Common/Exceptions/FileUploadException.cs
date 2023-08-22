namespace Application.Common.Exceptions;

public class FileUploadException : Exception
{
    public FileUploadException()
    {
    }

    public FileUploadException(string message)
        : base(message)
    {
    }

    public FileUploadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public FileUploadException(string name, object key)
        : base($"File \"{name}\" ({key}) had an error occur while uploading.")
    {
    }
}
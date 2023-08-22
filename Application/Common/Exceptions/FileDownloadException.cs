namespace Application.Common.Exceptions;

public class FileDownloadException : Exception
{
    public FileDownloadException()
    {
    }

    public FileDownloadException(string message)
        : base(message)
    {
    }

    public FileDownloadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public FileDownloadException(string name, object key)
        : base($"File \"{name}\" ({key}) had an error occur while downloading.")
    {
    }
}
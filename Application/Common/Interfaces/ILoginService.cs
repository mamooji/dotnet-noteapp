namespace Application.Common.Interfaces;

public interface ILoginResult
{
    public string AccessToken { get; set; }
    public int ExpiresIn { get; set; }
    public string UserName { get; set; }
    public string UserId { get; set; }
}

public interface ILoginService
{
    Task<ILoginResult> Login(string userName, string password, CancellationToken cancellationToken);
}
namespace Domain.Entities;

public class Account
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Type { get; set; }
    public string Provider { get; set; }
    public string ProviderAccountId { get; set; }
    public string RefreshToken { get; set; }
    public string AccessToken { get; set; }
    public string ExpiresAt { get; set; }
    public string TokenType { get; set; }
    public string Scope { get; set; }
    public string IdToken { get; set; }
    public string SessionState { get; set; }
    public User User { get; set; }
}
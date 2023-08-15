namespace Domain.Entities;

public class Session
{
    public string Id { get; set; }
    public string SessionToken { get; set; }
    public string UserId { get; set; }
    public DateTime Expires { get; set; }
    public User User { get; set; }
}
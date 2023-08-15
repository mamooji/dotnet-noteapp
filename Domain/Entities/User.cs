namespace Domain.Entities;

public class User
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime EmailVerifed { get; set; }
    public string Image { get; set; }

    public List<Account> Accounts { get; set; }
    public List<Session> Sessions { get; set; }
    public List<Note> Notes { get; set; }
}
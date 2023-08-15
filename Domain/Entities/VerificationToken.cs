namespace Domain.Entities;

public class VerificationToken
{
    public string Identifier { get; set; }
    public string Token { get; set; }
    private DateTime Expires { get; set; }
}
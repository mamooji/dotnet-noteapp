namespace Domain.Entities;

public class IdentityRole : Microsoft.AspNetCore.Identity.IdentityRole
{
    public IdentityRole()
    {
    }

    public IdentityRole(string roleName) : base(roleName)
    {
    }
}
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IUserService
{
    Task<ApplicationUser> Create(string email, string userName, string password,
        CancellationToken cancellationToken, string firstName = null, string lastName = null);

    Task<bool> DoesUserExist(string userName);
    Task<ApplicationUser> GetByUserName(string userName);
    string GeneratePassword();
    Task<string> SetUserRole(string userName, string setThisRole = null);
    Task<List<string>> SetUserRoles(string userName, List<string> roles);
}
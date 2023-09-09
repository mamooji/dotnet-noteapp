using System.Security.Claims;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IRoleService
{
    public Task AddClaimToRole(string claim, string role);

    public Task AddClaimsToRole(IEnumerable<string> claims, string role);

    public Task Create(IEnumerable<string> claims, string roleName);

    public Task Delete(string role);

    public Task DeleteClaimFromRole(string claim, string role);

    public Task DeleteClaimsFromRole(IEnumerable<string> claims, string role);

    public Task<bool> DoesClaimExist(string claim, string role);

    public Task<bool> DoesRoleExist(string role);

    public Task<List<IdentityRole>> GetAllRoles();


    public Task<List<Claim>> GetClaimsForRole(string role);

    public Task<string> GetRoleIdFromName(string role);
}
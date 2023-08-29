using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class UsernameAsPasswordValidator<TUser> : IPasswordValidator<TUser>
    where TUser : IdentityUser
{
    /// <summary>
    ///     Validates a password as an asynchronous operation.
    /// </summary>
    /// <param name="manager">
    ///     The <see cref="T:Microsoft.AspNetCore.Identity.UserManager`1" /> to retrieve the
    ///     <paramref name="user" /> properties from.
    /// </param>
    /// <param name="user">The user whose password should be validated.</param>
    /// <param name="password">The password supplied for validation</param>
    /// <returns>
    ///     The task object representing the asynchronous operation.
    /// </returns>
    public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
    {
        if (string.Equals(user.UserName, password, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "UsernameAsPassword",
                Description = "You cannot use your username as your password"
            }));
        return Task.FromResult(IdentityResult.Success);
    }
}
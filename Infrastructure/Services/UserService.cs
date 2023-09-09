using System.Security.Cryptography;
using System.Text;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IRoleService _roleService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager, IApplicationDbContext context,
        IRoleService roleService, SignInManager<ApplicationUser> signInManager,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _context = context;
        _roleService = roleService;
        _signInManager = signInManager;
        _logger = logger;
    }


    public async Task<ApplicationUser> Create(string email, string userName, string password,
        CancellationToken cancellationToken, string firstName = null, string lastName = null)
    {
        var appUser = new ApplicationUser
        {
            Email = email,
            UserName = userName,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await _userManager.CreateAsync(appUser, password);

        if (result.Succeeded == false)
        {
            var failures = result.Errors.Select(err => new ValidationFailure(err.Code, err.Description)).ToList();
            throw new ValidationException(failures);
        }

        var user = await _userManager.Users.Where(x => x.Email == email).FirstOrDefaultAsync(cancellationToken);

        if (user == null) throw new LogicalException($"User not found for email: {email} after creation");

        return appUser;
    }


    public async Task<bool> DoesUserExist(string userName)
    {
        return await _userManager.FindByNameAsync(userName) != null;
    }

    public async Task<ApplicationUser> GetByUserName(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);

        if (user == null) throw new NotFoundException();

        return user;
    }

    public string GeneratePassword()
    {
        const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string number = "1234567890";
        const string symbol = "!@#$%^&*()_+";

        var characterCategories = new List<string> { lowerCase, upperCase, number, symbol };
        var password = new StringBuilder();
        var rng = RandomNumberGenerator.Create();

        foreach (var characterCategory in characterCategories)
            for (var i = 0; i < 3; i++)
            {
                var randomNumber = new byte[1];
                rng.GetBytes(randomNumber);
                var index = randomNumber[0] % characterCategory.Length;
                password.Append(characterCategory[index]);
            }

        return password.ToString();
    }

    public async Task<string> SetUserRole(string userName, string setThisRole = null)
    {
        if (string.IsNullOrEmpty(setThisRole)) setThisRole = CustomRole.User;

        if (!_roleService.DoesRoleExist(setThisRole).Result)
            throw new NotFoundException($@"Role {setThisRole}, does not exist");

        var roleToUser = await _userManager.FindByNameAsync(userName);

        if (roleToUser == null) throw new NotFoundException();

        await _userManager.AddToRoleAsync(roleToUser, setThisRole);

        return setThisRole;
    }

    public async Task<List<string>> SetUserRoles(string userName, List<string> roles)
    {
        var resultingRoles = new List<string>();

        if (roles.Count == 0)
        {
            await SetUserRole(userName);
            return resultingRoles;
        }

        foreach (var role in roles) resultingRoles.Add(await SetUserRole(userName, role));

        return resultingRoles;
    }

    public async Task<bool> CheckPassword(ApplicationUser applicationUser, string password)
    {
        return await _userManager.CheckPasswordAsync(applicationUser, password);
    }


    private async Task<IdentityResult> GenerateTokenApplyReset(ApplicationUser user, string newPassword)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public static string HashString(string plainText)
    {
        StringBuilder sBuilder = new();

        foreach (var t in MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(plainText)))
            sBuilder.Append(t.ToString("x2"));

        return sBuilder.ToString();
    }
}
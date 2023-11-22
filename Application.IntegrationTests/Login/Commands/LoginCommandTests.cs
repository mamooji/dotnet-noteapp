using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Common.Exceptions;
using Application.Login.Commands.LoginCommand;
using FluentAssertions;
using NUnit.Framework;

namespace Backend.Application.IntegrationTests.Login.Commands;

using static Testing;

public class LoginCommandTests : TestBase
{
    private const string EMAIL_HOST = "@test.test";
    private const string PASSWORD = "S3tupT3$t!ng";
    private const string USER_NAME = "UserName";
    private string applicationUserId;

    [SetUp]
    public async Task SetUp()
    {
        await RemoveAllUsers();
        applicationUserId = await Testing.SeedUser(
            $"{Guid.NewGuid()}{EMAIL_HOST}",
            USER_NAME,
            PASSWORD
        );
    }

    [TearDown]
    public void TearDown()
    {
        LoginServiceLoggerMock.Invocations.Clear();
    }

    [Test]
    public async Task ItShouldLoginAUserWithCorrectInputs()
    {
        var loginCommand = new LoginCommand { UserName = USER_NAME, Password = PASSWORD };

        var result = await SendAsync(loginCommand);
        var context = GetContext();
        var user = context.Users.FirstOrDefault(u => u.UserName == USER_NAME);


        result.ExpiresIn.Should().Be(60 * 60 * 12);
        result.AccessToken.Should().NotBe(null);
        result.UserName.Should().NotBeEmpty();
        result.UserId.Should().NotBe(null);
    }

    [Test]
    public void ItShouldFailForInvalidUsername()
    {
        var loginCommand = new LoginCommand { UserName = "NotAUser", Password = PASSWORD };

        FluentActions
            .Invoking(() => SendAsync(loginCommand))
            .Should()
            .Throw<UnauthorizedException>();
    }

    [Test]
    public void ItShouldFailForInvalidPassword()
    {
        var loginCommand = new LoginCommand { UserName = USER_NAME, Password = "somePassword1@" };

        FluentActions
            .Invoking(() => SendAsync(loginCommand))
            .Should()
            .Throw<UnauthorizedException>();
    }

    [Test]
    public void ItShouldNotifyUserWhenLockedOut()
    {
        var loginCommand = new LoginCommand { UserName = USER_NAME, Password = "notthepassword" };

        FluentActions
            .Invoking(() => SendAsync(loginCommand))
            .Should()
            .Throw<UnauthorizedException>();

        FluentActions
            .Invoking(() => SendAsync(loginCommand))
            .Should()
            .Throw<UnauthorizedException>();

        FluentActions
            .Invoking(() => SendAsync(loginCommand))
            .Should()
            .Throw<UserLockedOutException>()
            .WithMessage(
                "Your account has been locked due to 3 invalid attempts. Please contact QUES support for help."
            );
    }

    [Test]
    public async Task ItShouldRemoveTwoFactorAuthClaim()
    {
        var claimToSeed = new Claim("TwoFactorVerified", "true");
        await SeedClaimForUser(applicationUserId, claimToSeed);

        var loginCommand = new LoginCommand { UserName = USER_NAME, Password = PASSWORD };

        await SendAsync(loginCommand);

        var userManager = GetUserManager();
        var applicationUser = await userManager.FindByIdAsync(applicationUserId);

        var claims = await userManager.GetClaimsAsync(applicationUser);
        claims
            .Should()
            .NotContain(uc => uc.Value == claimToSeed.Value && uc.Type == claimToSeed.Type);
    }
}
using Application.Common.Configurations;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Logging;
using Duende.IdentityServer.Models;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class LoginResult : ILoginResult
{
    public string AccessToken { get; set; }

    public int ExpiresIn { get; set; }

    public string UserName { get; set; }

    public string UserId { get; set; }
}

public class LoginService : ILoginService
{
    private readonly IApplicationDbContext _context;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IdentityServerConfiguration _identityServerConfiguration;
    private readonly ILogger<LoginService> _logger;
    private readonly IUserService _userService;
    private HttpClient _httpClient;


    public LoginService(IHttpClientFactory httpClientFactory,
        IOptions<IdentityServerConfiguration> identityServerConfiguration, ILogger<LoginService> logger,
        IUserService userService, IApplicationDbContext context, HttpClient httpClient)
    {
        _identityServerConfiguration = identityServerConfiguration.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _userService = userService;
        _context = context;
        _httpClient = httpClient;
    }

    public async Task<ILoginResult> Login(string userName, string password, CancellationToken cancellationToken)
    {
        var response = await RequestPasswordToken(userName, password, cancellationToken);

        var applicationUser = await _userService.GetByUserName(userName);
        if (applicationUser == null) throw new LogicalException();

        _logger.LogInformation(AppLogEvents.SecurityAudit, "Successful login attempt for user: {UserName}", userName);
        return new LoginResult
        {
            AccessToken = response.AccessToken,
            ExpiresIn = response.ExpiresIn,
            UserId = applicationUser.Id,
            UserName = applicationUser.UserName
        };
    }

    private async Task<TokenResponse> RequestPasswordToken(string userName, string password,
        CancellationToken cancellationToken)
    {
        _httpClient = _httpClientFactory.CreateClient("TokenClient");
        var response = await _httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            ClientId = _identityServerConfiguration.ClientId,
            ClientSecret = _identityServerConfiguration.ClientSecret,
            GrantType = GrantType.ResourceOwnerPassword,
            Scope = "api",
            UserName = userName,
            Password = password
        }, cancellationToken);

        if (response.IsError)
        {
            _logger.LogInformation(AppLogEvents.SecurityAudit, "Failed login attempt for user {userName}", userName);
            _logger.LogError(response.Exception, "Failed login: {response}", response);

            throw new UnauthorizedException();
        }

        return response;
    }
}
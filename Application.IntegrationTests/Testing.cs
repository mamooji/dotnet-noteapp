extern alias WebApiAlias;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.ServicesAndUtilities;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.Utility;
using Hangfire.SqlServer;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Seeders;
using Infrastructure.Services;
using Infrastructure.Services.Dao;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Respawn;
using Serilog;
using WebApiAlias::Backend.WebApi;
using IdentityRole = Domain.Entities.IdentityRole;

namespace Backend.Application.IntegrationTests;

[SetUpFixture]
public class Testing
{
    private static IConfigurationRoot _configuration;
    public static IServiceScopeFactory ScopeFactory;
    private static Checkpoint _checkpoint;
    private static ClaimsPrincipal _currentApplicationUser;
    public static readonly Mock<ILogger<LoginService>> LoginServiceLoggerMock = new();
    private static ServiceCollection _services;
    public static string CurrentApplicationUserId { get; private set; }

    public static string CurrentAuthToken { get; set; } = "foo.bar.baz";

    [OneTimeSetUp]
    [Obsolete]
    public async Task RunBeforeAnyTests()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables();

        _configuration = builder.Build();

        var webHostBuilder = new WebHostBuilder()
            .UseSerilog()
            .UseStartup<Startup>()
            .UseConfiguration(_configuration);

        var testServer = new TestServer(webHostBuilder);

        var startup = new Startup(_configuration);

        _services = new ServiceCollection();

        _services.AddSingleton(Mock.Of<IWebHostEnvironment>(w =>
            w.EnvironmentName == "Development" &&
            w.ApplicationName == "Backend.WebApi"));

        // _services.AddMediatR(Assembly.GetExecutingAssembly());

        startup.ConfigureServices(_services);

        _services.AddSingleton<IConfiguration>(provider => _configuration);

        ReplaceCurrentUserService();

        AddHttpTokenClient(testServer);

        ScopeFactory = _services.BuildServiceProvider().GetService<IServiceScopeFactory>();

        _checkpoint = new Checkpoint
        {
            SchemasToInclude = new[]
            {
                "dbo"
            },
            TablesToIgnore = new[]
            {
                "__EFMigrationsHistory",
                "application_user",
                "identity_role",
                "user",
                "identity_role_claim<string>",
                "identity_user_role<string>"
            },
            DbAdapter = DbAdapter.SqlServer
        };

        EnsureDatabase();
        await SeedDefaultRoleClaims();
        await SeedDefaultUsers();
    }

    public static IMapper GetMapper()
    {
        return ScopeFactory!.CreateScope().ServiceProvider.GetService<IMapper>()!;
    }

    private static void AddHttpTokenClient(TestServer testServer)
    {
        var tokenClient = testServer.CreateClient();
        tokenClient.BaseAddress = new Uri("http://localhost:5000/connect/token");
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(m => m.CreateClient(
                    "TokenClient"
                )
            )
            .Returns(tokenClient);

        // Register testing version
        _services.AddTransient(provider => httpClientFactory.Object);
    }

    private static void ReplaceCurrentUserService()
    {
        var currentUserServiceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ICurrentUserService));
        _services.Remove(currentUserServiceDescriptor);
        _services.AddTransient(_ =>
            Mock.Of<ICurrentUserService>(
                s =>
                    s.ApplicationUserId == CurrentApplicationUserId
                    && s.ApplicationUser == _currentApplicationUser
                    && s.AuthToken == CurrentAuthToken)
        );
    }

    public static async Task SeedDefaultUsers()
    {
        using var scope = ScopeFactory!.CreateScope();

        var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();

        var context = (ApplicationDbContext)GetContext();

        var defaultUserSeeder = new DefaultUserSeeder(context, userManager!);

        await defaultUserSeeder.Seed();
    }

    public static async Task RemoveAllUsers()
    {
        using var scope = ScopeFactory!.CreateScope();
        var context = (ApplicationDbContext)GetContext();

        await context.SaveChangesAsync();

        var users = await context.Users.ToListAsync();
        foreach (var user in users) context.Users.Remove(user);

        await context.SaveChangesAsync();
    }

    public static async Task<string> SeedUser(
        string email,
        string userName,
        string password,
        string firstName,
        string lastName,
        string phoneNumber 
    )
    {
        using var scope = ScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
        var context = (ApplicationDbContext)GetContext();

        var defaultUserSeeder = new DefaultUserSeeder(context, userManager);
        var applicationUserId = await defaultUserSeeder.SeedUser(
            email,
            userName,
            password,
            firstName: firstName,
            lastName: lastName,
            phoneNumber: phoneNumber
        );

        await context.SaveChangesAsync();

        return applicationUserId;
    }

    public static async Task SeedDefaultRoleClaims()
    {
        using var scope = ScopeFactory!.CreateScope();

        var roleService = scope.ServiceProvider.GetService<IRoleService>();
        var context = (ApplicationDbContext)GetContext();
        var defaultRoleClaimSeeder = new RoleClaimSeeder(context, roleService);

        await defaultRoleClaimSeeder.SeedAllRoleClaims();
    }

    public static async Task AddUserToRole(string userId, string role)
    {
        using var scope = ScopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(userId);

        await userManager.AddToRoleAsync(user, role);
    }

    public static async Task CreateRole(string roleName, List<string> claims)
    {
        using var scope = ScopeFactory.CreateScope();

        var roleService = scope.ServiceProvider.GetService<IRoleService>();

        await roleService.Create(claims, roleName);
    }

    // public static async Task<List<ApplicationUser>> GetAllUsers()
    // {
    //     using var scope = ScopeFactory.CreateScope();
    //     var userService = scope.ServiceProvider.GetService<IUserService>();
    //     return await userService.GetAllUsers();
    // }

    // get user roles
    public static async Task<List<string>> GetUserRoles(string userId)
    {
        using var scope = ScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);
        return new List<string>(await userManager.GetRolesAsync(user));
    }

    private static void EnsureDatabase()
    {
        using var scope = ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

        context.Database.Migrate();

        // This ensures the Hangfire schema is created
        new SqlServerStorage(_configuration.GetConnectionString("DefaultConnection"));
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = ScopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetService<IMediator>();

        return await mediator.Send(request);
    }

    public static async Task<string> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync("test@local", "Testing1234!", "TestFirstName", "TestLastName", "519-123-4567");
    }

    public static async Task<string> RunAsAdminAsync(bool twoFaVerified = true)
    {
        using var scope = ScopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
        var applicationUser = await userManager.FindByNameAsync("admin");
        if (applicationUser == null)
        {
            await SeedDefaultUsers();
            applicationUser = await userManager.FindByNameAsync("admin");
        }

        CurrentApplicationUserId = applicationUser.Id;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, applicationUser.Id),
            new(ClaimTypes.Role, CustomRole.Admin)
        };

        if (twoFaVerified) await userManager.AddClaimAsync(applicationUser, new Claim("TwoFactorVerified", "true"));

        _currentApplicationUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
        return applicationUser.Id;
    }

    public static async Task<string> RunAsUserAsync(
        string userName,
        string password,
        string firstName,
        string lastName,
        string phoneNumber,
        List<string> roleNames = null,
        string email = null,
        bool twoFaVerified = true
        
    )
    {
        var userEmail = email ?? $"{userName}@vehikl.com";
        CurrentApplicationUserId = await SeedUser(userEmail, userName, password, firstName, lastName, phoneNumber);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, CurrentApplicationUserId)
        };

        if (roleNames != null) claims.AddRange(roleNames.Select(rn => new Claim(ClaimTypes.Role, rn)));

        if (twoFaVerified)
        {
            var userManager = GetUserManager();
            var applicationUser = await userManager.FindByIdAsync(CurrentApplicationUserId);
            await userManager.AddClaimAsync(applicationUser, new Claim("TwoFactorVerified", "true"));
        }

        _currentApplicationUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        return CurrentApplicationUserId;
    }

    public static async Task SeedUserRoleForCurrentUser(string roleName)
    {
        var existingClaims = _currentApplicationUser.Claims;
        var roleClaim = new Claim(ClaimTypes.Role, roleName);
        var newClaims = existingClaims.Concat(new List<Claim> { roleClaim });
        _currentApplicationUser = new ClaimsPrincipal(new ClaimsIdentity(newClaims, "mock"));

        await SeedUserRoleByName(CurrentApplicationUserId, roleName);
    }

    public static async Task SeedUserRoleByName(string userId, string roleName)
    {
        var userManager = ScopeFactory.CreateScope().ServiceProvider.GetService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);
        await userManager.AddToRoleAsync(user, roleName);
    }


    public static async Task SeedUserRole(string userId, string roleId)
    {
        var roleManager = ScopeFactory.CreateScope().ServiceProvider.GetService<RoleManager<IdentityRole>>();
        var role = await roleManager.FindByIdAsync(roleId);
        await SeedUserRoleByName(userId, role.Name);
    }

    public static async Task ResetState()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        await using (var conn = new SqlConnection(connectionString))
        {
            await conn.OpenAsync();

            await _checkpoint.Reset(conn);
        }

        CurrentApplicationUserId = null;
        _currentApplicationUser = null;
    }

    public static async Task<TEntity> FindAsync<TEntity>(int id)
        where TEntity : class
    {
        using var scope = ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(id);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static IApplicationDbContext GetContext(IServiceScope scope = null)
    {
        var context = (scope ?? ScopeFactory.CreateScope()).ServiceProvider.GetService<ApplicationDbContext>();
        return context;
    }

    public static Microsoft.AspNetCore.Identity.RoleManager<IdentityRole> GetRoleManager(IServiceScope scope = null)
    {
        return (scope ?? ScopeFactory.CreateScope()).ServiceProvider.GetService<RoleManager<IdentityRole>>();
    }

    public static UserManager<ApplicationUser> GetUserManager(IServiceScope scope = null)
    {
        return (scope ?? ScopeFactory.CreateScope()).ServiceProvider.GetService<UserManager<ApplicationUser>>();
    }

    public static IDaoService GetDaoService()
    {
        return new DaoService(ScopeFactory, GetContext(), new MapperUtility());
    }

    public static async Task ExecuteNonQuery(string sql)
    {
        var dao = GetDaoService();
        await dao.ExecuteNonQueryUsingInlineSql(sql, CancellationToken.None);
    }

    /// <summary>
    ///     Gets the role service reference for unit testing.
    /// </summary>
    /// <returns>
    ///     IRoleService reference, to access role service functions.
    /// </returns>
    public static IRoleService GetRoleService(IServiceScope scope = null)
    {
        return (scope ?? ScopeFactory.CreateScope()).ServiceProvider.GetService<IRoleService>();
    }

    /// <summary>
    ///     Gets the user service reference for unit testing.
    /// </summary>
    /// <returns>
    ///     IUserService reference, to access user service functions.
    /// </returns>
    public static IUserService GetUserService(IServiceScope scope = null)
    {
        return (scope ?? ScopeFactory.CreateScope()).ServiceProvider.GetService<IUserService>();
    }

    public static T RetrieveValueFromConfig<T>(string valuekey, string sectionKey)
    {
        var quesConfig = _configuration.GetSection(sectionKey);
        var value = quesConfig.GetValue<T>(valuekey);

        return value;
    }
}
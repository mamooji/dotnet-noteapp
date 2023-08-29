using System.Security.Claims;
using Application.Common.Authorization;
using Application.Common.Configurations;
using Application.Common.Interfaces;
using Domain.Entities;
using Duende.IdentityServer.Models;
using Hangfire;
using Hangfire.MemoryStorage;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Npgsql;
using Serilog;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration, IdentityServerConfiguration identityServerConfiguration, bool isTest = false)
    {
        var config = identityServerConfiguration;

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        NpgsqlConnection.GlobalTypeMapper.UseJsonNet(null, null, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        });

        services.AddScoped<IApplicationDbContext, ApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 12;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromDays(365 * 99); // Lockout for 99 years, expressed as days.
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddPasswordValidator<UsernameAsPasswordValidator<ApplicationUser>>();

        if (!isTest)
            services.AddLogging(cfg =>
            {
                cfg.ClearProviders();
                cfg.AddSerilog(dispose: true);
            });


        services.AddIdentityServer()
            .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options =>
            {
                options.Clients.Add(new Client
                {
                    ClientId = config.ClientId,
                    AllowedGrantTypes = { GrantType.ResourceOwnerPassword },
                    ClientSecrets = { new Secret(config.ClientSecret.Sha256()) },
                    AllowedScopes = { "api" },
                    AccessTokenLifetime = config.AccessTokenLifetime
                });

                foreach (var resource in config.Resources)
                    options.ApiResources.Add(new ApiResource
                    {
                        Name = resource.Name,
                        Scopes = resource.Scopes
                    });
            });

        services.AddAuthentication(options => { options.DefaultAuthenticateScheme = "Bearer"; })
            .AddIdentityServerJwt()
            .AddJwtBearer(options =>
            {
                options.Authority = config.BaseUrl;
                options.Audience = config.Audience;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(nameof(HasClaim), policy => { policy.Requirements.Add(new HasClaim()); });
        });

        //Add Hangfire to services:
        GlobalConfiguration.Configuration
            .UseSqlServerStorage(connectionString); //Global connection string for Hangfire

        GlobalConfiguration.Configuration.UseActivator(
            new HangfireActivator(services.BuildServiceProvider())); //Create a Job Activator that uses IoC

        services.AddHangfire(x =>
        {
            x.UseMemoryStorage();
            x.UseSqlServerStorage(connectionString);
            x.UseSerializerSettings(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
        });

        services.AddHangfireServer();

        return services;
    }
}

public class HangfireJobActivatorScope : JobActivatorScope
{
    private readonly List<IDisposable> _disposables = new();
    private readonly IServiceScope _serviceScope;

    public HangfireJobActivatorScope(IServiceScope serviceScope)
    {
        _serviceScope = serviceScope;
    }

    public override object Resolve(Type type)
    {
        var instance = _serviceScope.ServiceProvider.GetService(type);

        if (instance is IDisposable disposable) _disposables.Add(disposable);

        return instance;
    }

    public override void DisposeScope()
    {
        foreach (var disposable in _disposables) disposable.Dispose();

        _serviceScope.Dispose();
    }
}

public class HangfireActivator : JobActivator
{
    private readonly IServiceProvider _serviceProvider;

    public HangfireActivator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override JobActivatorScope BeginScope(JobActivatorContext context)
    {
        var serviceScope = _serviceProvider.CreateScope();
        return new HangfireJobActivatorScope(serviceScope);
    }
}
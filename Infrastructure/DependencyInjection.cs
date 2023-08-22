using Application.Common.Interfaces;
using Hangfire;
using Hangfire.MemoryStorage;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Npgsql;
using Serilog;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration? configuration,
        bool isTest = false)
    {
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

        if (!isTest)
            services.AddLogging(cfg =>
            {
                cfg.ClearProviders();
                cfg.AddSerilog(dispose: true);
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
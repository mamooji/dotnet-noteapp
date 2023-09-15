using Application;
using Application.Common.Configurations;
using Application.Common.Interfaces;
using Backend.WebApi.Services;
using Hangfire;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NSwag;
using NSwag.Generation.Processors.Security;
using Serilog;
using WebApi.Filters;
using WebApi.Middleware;
using DependencyInjection = Infrastructure.DependencyInjection;

namespace Backend.WebApi;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        var identityServerConfigurationSection = Configuration.GetSection("IdentityServerConfiguration");

        services.Configure<IdentityServerConfiguration>(identityServerConfigurationSection);

        var identityServerConfiguration = identityServerConfigurationSection.Get<IdentityServerConfiguration>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddAutoMapper(typeof(Startup), typeof(DependencyInjection));
        services.AddApplication();

        services.AddCors(options => { options.AddPolicy("CorsPolicy", GetCorsPolicy()); });

        services.AddInfrastructure(Configuration, identityServerConfiguration);

        services.AddRouting(options => options.LowercaseUrls = true);

        services.AddHttpContextAccessor();

        // services.AddMvc();

        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

        services.AddControllersWithViews(options =>
                options.Filters.Add(
                    new ApiExceptionFilter(new Logger<ApiExceptionFilter>(new LoggerFactory()))))
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
            });

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

        services.AddOpenApiDocument(configure =>
        {
            configure.Title = "Backend API";
            configure.Version = "v1";
            configure.Description = "and Dotnet backend for my notes app";
            configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });
            configure.AllowReferencesWithProperties = true;
            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseHangfireDashboard();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseMiddleware<ExceptionMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseHealthChecks("/health");
        app.UseStaticFiles();

        bool.TryParse(Environment.GetEnvironmentVariable("SHOW_SWAGGER"), out var showSwagger);
        if (env.IsDevelopment() || showSwagger)
            app.UseSwaggerUi3(settings =>
            {
                settings.Path = "/api";
                settings.DocumentPath = "/api/specification.json";
            });

        app.UseRouting();
        app.UseCors("CorsPolicy");
        app.UseAuthentication();
        app.UseIdentityServer();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                "default",
                "{controller}/{action=Index}/{id?}");
            endpoints.MapRazorPages();
        });
    }


    private CorsPolicy GetCorsPolicy()
    {
        var corsBuilder = new CorsPolicyBuilder();
        corsBuilder.AllowAnyOrigin();
        corsBuilder.AllowAnyHeader();
        corsBuilder.AllowAnyMethod();
        corsBuilder.WithExposedHeaders("Content-Disposition");
        return corsBuilder.Build();
    }
}
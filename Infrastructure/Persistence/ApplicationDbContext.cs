using System.Data;
using System.Reflection;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Extensions;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Options;
using Infrastructure.Persistence.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, IApplicationDbContext,
    IPersistedGrantDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IOptions<OperationalStoreOptions> _operationalStoreOptions;
    private IDbContextTransaction _currentTransaction;

    public ApplicationDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions,
        ICurrentUserService currentUserService) :
        base(options)
    {
        _currentUserService = currentUserService;
        _operationalStoreOptions = operationalStoreOptions;
    }

    public DbSet<Note> Note { get; set; }

    public string ConnectionString => Database.GetConnectionString();

    public async Task<int> SaveChangesAsync()
    {
        return await SaveChangesAsync(CancellationToken.None);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUserService.ApplicationUserId;
                    entry.Entity.Created = utcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = _currentUserService.ApplicationUserId;
                    entry.Entity.LastModified = utcNow;
                    break;
            }

        return await base.SaveChangesAsync(cancellationToken);
    }


    public DbSet<PersistedGrant> PersistedGrants { get; set; }

    public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }
    public DbSet<Key> Keys { get; set; }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null) return;

        _currentTransaction = await base.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted)
            .ConfigureAwait(false);
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync().ConfigureAwait(false);

            _currentTransaction?.Commit();
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task Ping()
    {
        await using var connection = Database.GetDbConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 + 1";
        var result = (await command.ExecuteScalarAsync()).ToString();
        int.TryParse(result, out var dbResult);

        if (dbResult != 2) throw new Exception("Database is not connected");
    }

    public void ForceReloadFromDatabase<T>(T entity) where T : class
    {
        Entry(entity).State = EntityState.Detached;
    }

    public void ForceUpdateToDatabase<T>(T entity) where T : class
    {
        Entry(entity).State = EntityState.Modified;
    }

    [Obsolete]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    protected override void OnModelCreating(ModelBuilder builder)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
        builder.ConfigurePersistedGrantContext(_operationalStoreOptions.Value);
        builder.SnakeCaseModels();
    }
}
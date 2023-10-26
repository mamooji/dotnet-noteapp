using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext : IDisposable
{
    DbSet<Note> Note { get; set; }
    DbSet<ApplicationUser> Users { get; set; }
    string ConnectionString { get; }
    DbSet<IdentityRole> Roles { get; set; }
    Task<int> SaveChangesAsync();
    Task<int> SaveSeededChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    void RollbackTransaction();
    Task Ping();
    void ForceReloadFromDatabase<T>(T entity) where T : class;
    void ForceUpdateToDatabase<T>(T entity) where T : class;
}
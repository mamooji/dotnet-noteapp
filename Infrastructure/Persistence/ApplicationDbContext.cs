using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public DbSet<Account> Account { get; set; }
    public DbSet<Note> Note { get; set; }
    public DbSet<Session> Session { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<VerificationToken> VerificationToken { get; set; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(cancellationToken);
    }
}
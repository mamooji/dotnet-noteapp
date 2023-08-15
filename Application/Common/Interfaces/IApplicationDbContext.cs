using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext : IDisposable
{
    DbSet<Account> Account { get; set; }
    DbSet<Note> Note { get; set; }
    DbSet<Session> Session { get; set; }
    DbSet<User> User { get; set; }
    DbSet<VerificationToken> VerificationToken { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
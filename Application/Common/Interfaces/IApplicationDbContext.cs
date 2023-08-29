using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext : IDisposable
{
    DbSet<Note> Note { get; set; }
    DbSet<ApplicationUser> ApplicationUsers { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
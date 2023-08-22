using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class SqlDatabaseSeedAsync
{
    public static async Task MigrateDatabase(IApplicationDbContext ctx)
    {
        var context = (ApplicationDbContext)ctx;

        await context.Database.MigrateAsync(CancellationToken.None);
    }
}
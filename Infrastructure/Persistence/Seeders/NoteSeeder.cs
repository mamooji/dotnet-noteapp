using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeders;

public class NoteSeeder
{
    private readonly IApplicationDbContext _context;

    public NoteSeeder(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Seed()
    {
        var adminUserId =
            await _context.Users.Where(u => u.UserName == "admin").Select(u => u.Id).FirstOrDefaultAsync();

        if (adminUserId != null)
        {
            var notesList = new List<Note>
            {
                new()
                {
                    ApplicationUserId = adminUserId,
                    Title = "Test Title",
                    Body = "Test Body",
                    CreatedBy = adminUserId,
                    LastModifiedBy = adminUserId
                },
                new()
                {
                    ApplicationUserId = adminUserId,
                    Title = "Test Title two",
                    Body = "Test Body two",
                    CreatedBy = adminUserId,
                    LastModifiedBy = adminUserId
                }
            };
            await _context.Note.AddRangeAsync(notesList, CancellationToken.None);
        }

        await _context.SaveSeededChangesAsync();
    }
}
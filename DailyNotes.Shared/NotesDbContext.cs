using Microsoft.EntityFrameworkCore;
using DailyNotes.Shared;

namespace DailyNotes.Api.Data;

public class NotesDbContext : DbContext
{
    public NotesDbContext(DbContextOptions<NotesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Note> Notes => Set<Note>();
}
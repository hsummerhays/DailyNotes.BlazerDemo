using Microsoft.EntityFrameworkCore;
using DailyNotes.Shared.Models;

namespace DailyNotes.Api.Data;

public class NotesDbContext : DbContext
{
    public NotesDbContext(DbContextOptions<NotesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Note> Notes { get; set; }
    public DbSet<WorkDay> WorkDays { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<WorkTask> WorkTasks { get; set; }
    public DbSet<TenantUser> TenantUsers { get; set; }
}
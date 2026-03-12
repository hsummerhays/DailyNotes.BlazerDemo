using DailyNotes.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using DailyNotes.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var azureAdConfig = builder.Configuration.GetSection("AzureAd");
if (builder.Environment.IsDevelopment() && (string.IsNullOrEmpty(azureAdConfig["ClientId"]) || azureAdConfig["ClientId"].Contains("[")))
{
    builder.Services.AddAuthentication("Demo")
        .AddScheme<AuthenticationSchemeOptions, MockAuthHandler>("Demo", null);
}
else
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(azureAdConfig);
}

builder.Services.AddAuthorization();

builder.Services.AddDbContext<NotesDbContext>(options =>
    options.UseSqlite("Data Source=notes.db"));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotesDbContext>();
    db.Database.EnsureCreated();
    Console.WriteLine("Database ensured created.");
    
    // Seed Sample Data for User
    var targetUserId = "072fbde7-eae8-4aee-b373-8ac17e74aba1";
    var userExists = db.TenantUsers.Any(tu => tu.UserId == targetUserId);
    Console.WriteLine($"Seeding check: User {targetUserId} exists: {userExists}");

    if (!userExists)
    {
        Console.WriteLine("Starting database seeding...");
        db.TenantUsers.Add(new TenantUser 
        { 
            TenantId = 1, 
            UserId = targetUserId, 
            Role = "admin", 
            CreatedAt = DateTime.UtcNow 
        });

        // Seed Projects
        var productLaunch = new Project { TenantId = 1, UserId = targetUserId, Name = "Product Launch 2024", Category = "Marketing", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var infraMigration = new Project { TenantId = 1, UserId = targetUserId, Name = "Infrastructure Migration", Category = "IT", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Projects.AddRange(productLaunch, infraMigration);
        db.SaveChanges(); // Need IDs for tasks

        // Seed Work Days
        var today = new WorkDay { TenantId = 1, UserId = targetUserId, WorkDate = DateTime.Today, TimeIn1 = DateTime.Today.AddHours(9), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.WorkDays.Add(today);

        // Seed Tasks
        db.WorkTasks.AddRange(
            new WorkTask { TenantId = 1, UserId = targetUserId, ProjectId = productLaunch.Id, Name = "Draft social media announcement", Status = "todo", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new WorkTask { TenantId = 1, UserId = targetUserId, ProjectId = productLaunch.Id, Name = "Prepare slide deck for kickoff", Status = "in-progress", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new WorkTask { TenantId = 1, UserId = targetUserId, ProjectId = infraMigration.Id, Name = "Audit cloud resource usage", Status = "done", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        // Seed Notes
        db.Notes.AddRange(
            new Note { TenantId = 1, UserId = targetUserId, NoteDate = DateTime.Today, Content = "Met with the dev team to discuss the API migration. Everyone is aligned on the new architecture.", IsPinned = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Note { TenantId = 1, UserId = targetUserId, NoteDate = DateTime.Today.AddDays(-1), Content = "Initial research for the new Blazor dashboard. Glassmorphism looks like the way to go.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        db.SaveChanges();
        Console.WriteLine("Database seeding completed successfully.");
    }
    else
    {
        Console.WriteLine($"Database already seeded for user: {targetUserId}");
        var projectCount = db.Projects.Count(p => p.UserId == targetUserId);
        var noteCount = db.Notes.Count(n => n.UserId == targetUserId);
        Console.WriteLine($"Stats for user: Projects={projectCount}, Notes={noteCount}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try 
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"CRITICAL ERROR DURING RUN: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    throw;
}

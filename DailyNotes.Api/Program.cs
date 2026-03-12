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
    
    // Seed Demo User if doesn't exist
    if (!db.TenantUsers.Any(tu => tu.UserId == "demo-user-oid"))
    {
        db.TenantUsers.Add(new TenantUser 
        { 
            TenantId = 1, 
            UserId = "demo-user-oid", 
            Role = "admin", 
            CreatedAt = DateTime.UtcNow 
        });
        db.SaveChanges();
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

app.Run();

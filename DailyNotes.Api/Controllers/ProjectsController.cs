using Microsoft.AspNetCore.Mvc;
using DailyNotes.Api.Data;
using DailyNotes.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

namespace DailyNotes.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly NotesDbContext _context;

    public ProjectsController(NotesDbContext context)
    {
        _context = context;
    }

    private async Task<(int TenantId, string UserId)> GetUserContext()
    {
        var userId = User.GetObjectId();
        if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException();

        var tenantUser = await _context.TenantUsers
            .FirstOrDefaultAsync(tu => tu.UserId == userId);
        
        return (tenantUser?.TenantId ?? 1, userId);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> Get()
    {
        var (tenantId, userId) = await GetUserContext();
        return await _context.Projects
            .Where(p => p.TenantId == tenantId && p.UserId == userId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Project>> Post(Project project)
    {
        var (tenantId, userId) = await GetUserContext();
        project.TenantId = tenantId;
        project.UserId = userId;
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Project project)
    {
        var (tenantId, userId) = await GetUserContext();
        if (id != project.Id) return BadRequest();

        var existing = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && p.UserId == userId);

        if (existing == null) return NotFound();

        existing.Name = project.Name;
        existing.Category = project.Category;
        existing.Visibility = project.Visibility;
        existing.CreatedDate = project.CreatedDate;
        existing.CompletedDate = project.CompletedDate;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }
}

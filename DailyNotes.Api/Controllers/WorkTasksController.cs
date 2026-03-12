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
public class WorkTasksController : ControllerBase
{
    private readonly NotesDbContext _context;

    public WorkTasksController(NotesDbContext context)
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
    public async Task<ActionResult<IEnumerable<WorkTask>>> Get()
    {
        try 
        {
            var (tenantId, userId) = await GetUserContext();
            return await _context.WorkTasks
                .Where(t => t.TenantId == tenantId && t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in WorkTasksController.Get: {ex.Message}");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("projects")]
    public async Task<IEnumerable<Project>> GetProjects()
    {
        var (tenantId, userId) = await GetUserContext();
        return await _context.Projects
            .Where(p => p.TenantId == tenantId && p.UserId == userId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<WorkTask> Post(WorkTask task)
    {
        var (tenantId, userId) = await GetUserContext();
        task.TenantId = tenantId;
        task.UserId = userId;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var (tenantId, userId) = await GetUserContext();
        var task = await _context.WorkTasks
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId && t.UserId == userId);

        if (task == null) return NotFound();

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(task);
    }
}

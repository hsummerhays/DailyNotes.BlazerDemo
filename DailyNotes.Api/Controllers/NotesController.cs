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
public class NotesController : ControllerBase
{
    private readonly NotesDbContext _context;

    public NotesController(NotesDbContext context)
    {
        _context = context;
    }

    private async Task<(int TenantId, string UserId)> GetUserContext()
    {
        var userId = User.GetObjectId();
        if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException();

        var tenantUser = await _context.TenantUsers
            .FirstOrDefaultAsync(tu => tu.UserId == userId);
        
        if (tenantUser == null)
        {
            // For now, auto-assign to a default tenant or handle as needed
            // In a real app, this would be part of a registration/invitation flow
            return (1, userId); 
        }

        return (tenantUser.TenantId, userId);
    }

    [HttpGet]
    public async Task<IEnumerable<Note>> Get()
    {
        var (tenantId, userId) = await GetUserContext();
        return await _context.Notes
            .Where(n => n.TenantId == tenantId && n.UserId == userId)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<Note> Post(Note note)
    {
        var (tenantId, userId) = await GetUserContext();
        note.TenantId = tenantId;
        note.UserId = userId;
        note.CreatedAt = DateTime.UtcNow;
        note.UpdatedAt = DateTime.UtcNow;

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();
        return note;
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (tenantId, userId) = await GetUserContext();
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == tenantId && n.UserId == userId);

        if (note != null)
        {
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return Ok();
        }

        return NotFound();
    }
}
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
        Console.WriteLine($"API Request from User OID: {userId}");
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
    public async Task<IEnumerable<Note>> Get([FromQuery] int? taskId = null)
    {
        var (tenantId, userId) = await GetUserContext();
        var query = _context.Notes
            .Where(n => n.TenantId == tenantId && n.UserId == userId);
        
        if (taskId.HasValue)
        {
            query = query.Where(n => n.WorkTaskId == taskId.Value);
        }

        return await query.OrderByDescending(n => n.NoteDate).ToListAsync();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Note note)
    {
        var (tenantId, userId) = await GetUserContext();
        if (id != note.Id) return BadRequest();

        var existing = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == tenantId && n.UserId == userId);

        if (existing == null) return NotFound();

        existing.Content = note.Content;
        existing.NoteDate = note.NoteDate;
        existing.WorkTaskId = note.WorkTaskId;
        existing.TimeMinutes = note.TimeMinutes;
        existing.IsPinned = note.IsPinned;
        existing.Visibility = note.Visibility;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(existing);
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
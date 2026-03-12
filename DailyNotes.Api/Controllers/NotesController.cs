using Microsoft.AspNetCore.Mvc;
using DailyNotes.Api.Data;
using DailyNotes.Shared;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly NotesDbContext _context;

    public NotesController(NotesDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IEnumerable<Note>> Get()
    {
        return await _context.Notes.ToListAsync();
    }

    [HttpPost]
    public async Task<Note> Post(Note note)
    {
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();
        return note;
    }

    [HttpDelete("{id}")]
    public async Task Delete(int id)
    {
        var note = await _context.Notes.FindAsync(id);

        if (note != null)
        {
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
        }
    }
}
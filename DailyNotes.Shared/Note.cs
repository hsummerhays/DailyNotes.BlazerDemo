namespace DailyNotes.Shared;

public class Note
{
    public int Id { get; set; }

    public string Text { get; set; } = "";

    public DateTime Created { get; set; } = DateTime.UtcNow;
}

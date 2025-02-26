namespace CyclingStats.Models;

public class UserGameDetails
{
    public List<UserGameResult> Results { get; set; } = new();
    public string? Comment { get; set; }
    public DateTime Created { get; set; }
    public GameStatus Status { get; set; }
}
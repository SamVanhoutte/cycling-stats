namespace CyclingStats.Models;

public class PlayerWatcher
{
    public string? PlayerUserId { get; set; }
    public string? Name { get; set; }
    public string WcsUserName { get; set; }
    public bool MainOpponent { get; set; }
    public string Comment { get; set; }
    public DateTime Created { get; set; }
    public UserDetails? Player { get; set; }
}
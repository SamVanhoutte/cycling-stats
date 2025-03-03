namespace CyclingStats.Models;

public class UserGameTeam
{
    public string Name { get; set; }
    public string Comment { get; set; }
    public DateTime LastSaved { get; set; }
    public List<TeamRider>? Riders { get; set; }
    public string TeamId { get; set; }
}
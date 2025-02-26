namespace CyclingStats.Models;

public class GameCompetitor
{
    public string UserId { get; set; }
    public string WcsUserName { get; set; }
    public bool MainCompetitor { get; set; }
    public DateTime Updated { get; set; }
}
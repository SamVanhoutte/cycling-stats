namespace CyclingStats.Models;

public class StartGrid
{
    public int StarBudget { get; set; }
    public GameStatus Status { get; set; }
    public List<StartingRider> Riders { get; set; }
    public DateTime Updated { get; set; }
}
namespace CyclingStats.Models;

public class StartGrid
{
    public int StarBudget { get; set; }
    public List<(Rider, int)> Riders { get; set; }
}
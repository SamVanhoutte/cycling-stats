namespace CyclingStats.Models;

public class RacerRacePoint
{
    public Rider Rider { get; set; }
    public int Points { get; set; }
    public int Position { get; set; }
    public int Pc { get; set; }
    public int Mc { get; set; }
    public int Picked { get; set; }
    public int Stars { get; set; }
}
namespace CyclingStats.WebApi.Models;

public class RaceDurationResult
{
    public string RaceId { get; set; }
    public int DurationSeconds { get; set; }
    public int Distance { get; set; }
    public decimal AverageSpeedKh { get; set; }
}
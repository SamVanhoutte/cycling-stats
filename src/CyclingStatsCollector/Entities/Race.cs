using Microsoft.EntityFrameworkCore;

namespace CyclingStatsCollector.Entities;

[PrimaryKey("Id")]
public class Race
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime RaceDate { get; set; }
    public string RaceType { get; set; }
    public double Distance { get; set; }
    public ICollection<Result> Results { get; set; }
}
using Microsoft.EntityFrameworkCore;

namespace CyclingStatsCollector.Entities;


[PrimaryKey("Id")]
public class Rider
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Team { get; set; }
    public int Sprinter { get; set; }
    public int Puncheur { get; set; }
    public int OneDay { get; set; }
    public int Climber { get; set; }
    public int AllRounder { get; set; }
    public int TimeTrialist { get; set; }
}
using Microsoft.EntityFrameworkCore;

namespace CyclingStats.DataAccess.Entities;

[PrimaryKey("TeamId")]
public class UserTeam
{
    public Guid TeamId { get; set; }
    public string GameId { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Comment { get; set; }
    public DateTime Created { get; set; }
    public string TeamListJson { get; set; }
    public virtual User User { get; set; }
    public virtual Game Game { get; set; }
}
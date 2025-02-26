using Microsoft.EntityFrameworkCore;

namespace CyclingStats.DataAccess.Entities;

[PrimaryKey( "UserId", "GameId")]
public class UserGame
{
    public string GameId { get; set; }
    public string UserId { get; set; }
    public string Comment { get; set; }
    public DateTime Created { get; set; }
    public virtual User User { get; set; }
    public virtual Game Game { get; set; }
}
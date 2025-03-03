namespace CyclingStats.Models;

public class UserDetails
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string? AuthenticationId { get; set; }
    public string? WcsUserName { get; set; }
    public string? Language { get; set; }
    public string? Phone { get; set; }
    public DateTime Updated { get; set; }
}
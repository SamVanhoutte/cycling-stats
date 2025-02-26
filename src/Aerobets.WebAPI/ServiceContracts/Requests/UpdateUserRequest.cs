namespace Aerobets.WebAPI.ServiceContracts.Requests;

public class UpdateUserRequest
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? WcsUserName { get; set; }
    public string? Language { get; set; }
    public string? AuthenticationId { get; set; }
}
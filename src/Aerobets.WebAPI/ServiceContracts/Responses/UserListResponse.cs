using Aerobets.WebAPI.ServiceContracts.Structs;

namespace Aerobets.WebAPI.ServiceContracts.Responses;

public class UserListResponse(User[] users)
{
    public long Count { get; set; } = users.Length;
    public User[] Users { get; set; } = users;
}
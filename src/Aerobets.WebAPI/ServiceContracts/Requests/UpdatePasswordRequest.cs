namespace Aerobets.WebAPI.ServiceContracts.Requests;

public class UpdatePasswordRequest
{
    public string Username { get; set; }
    public string EncryptedPassword { get; set; }
}
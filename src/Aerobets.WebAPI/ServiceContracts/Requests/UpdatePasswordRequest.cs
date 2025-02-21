namespace Aerobets.WebAPI.ServiceContracts.Requests;

public class UpdatePasswordRequest
{
    public string EncryptedPassword { get; set; }
}
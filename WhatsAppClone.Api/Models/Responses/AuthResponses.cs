namespace WhatsAppClone.Api.Models;

public record UserDto(long UserId, string PhoneNumber, string? DisplayName, string? About, string? ProfilePhotoUrl);
public record AuthResult(string AccessToken, string RefreshToken, UserDto User, bool IsNewUser);

// usp_User_GetOrCreateByPhone ka result: User (table props) + IsNewUser (table column nahi hai)
public class UserGetOrCreateResult : User
{
    public bool IsNewUser { get; set; }
}

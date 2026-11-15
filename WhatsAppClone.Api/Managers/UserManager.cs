using WhatsAppClone.Api.Interface;
using WhatsAppClone.Api.Models;
using WhatsAppClone.Api.Repositories;

namespace WhatsAppClone.Api.Managers;

public class UserManager(IUserRepository users) : IUserManager
{
    private static UserDto ToDto(User u) =>
        new(u.UserId, u.PhoneNumber, u.DisplayName, u.About, u.ProfilePhotoUrl);

    public async Task<ApiResponse<UserDto>> GetMeAsync(long userId)
    {
        var u = await users.GetByIdAsync(userId);
        return u is null ? ApiResponse<UserDto>.Fail("User not found.") : ApiResponse<UserDto>.Ok(ToDto(u));
    }

    public async Task<ApiResponse<UserDto>> UpdateProfileAsync(long userId, UpdateProfileRequest req)
    {
        var name = req.DisplayName?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 100)
            return ApiResponse<UserDto>.Fail("Display name is required (max 100 chars).");
        if (req.About?.Length > 200)
            return ApiResponse<UserDto>.Fail("About max 200 chars.");

        var u = await users.UpdateProfileAsync(userId, name, req.About?.Trim(), req.ProfilePhotoUrl);
        return u is null ? ApiResponse<UserDto>.Fail("User not found.") : ApiResponse<UserDto>.Ok(ToDto(u));
    }
}
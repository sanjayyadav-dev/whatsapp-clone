using WhatsAppClone.Api.Models;

namespace WhatsAppClone.Api.Interface
{
    public interface IUserManager
    {
        Task<ApiResponse<UserDto>> GetMeAsync(long userId);
        Task<ApiResponse<UserDto>> UpdateProfileAsync(long userId, UpdateProfileRequest req);
    }
}

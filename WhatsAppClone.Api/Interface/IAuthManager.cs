using WhatsAppClone.Api.Models;

namespace WhatsAppClone.Api.Interface;

public interface IAuthManager
{
    Task<ApiResponse<string>> SendOtpAsync(SendOtpRequest req);
    Task<ApiResponse<AuthResult>> VerifyOtpAsync(VerifyOtpRequest req);
    Task<ApiResponse<AuthResult>> RefreshAsync(RefreshTokenRequest req);
    Task<ApiResponse<string>> LogoutAsync(long userId, string deviceId);
}
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppClone.Mobile.Models
{
    public class ApiModels
    {

        public record ApiResponse<T>(bool Success, T? Data, string? Error);
        public record UserDto(long UserId, string PhoneNumber, string? DisplayName, string? About, string? ProfilePhotoUrl);
        public record AuthResult(string AccessToken, string RefreshToken, UserDto User, bool IsNewUser);

        public record SendOtpRequest(string PhoneNumber);
        public record VerifyOtpRequest(string PhoneNumber, string Otp, string DeviceUniqueId, string Platform, string? PushToken);
        public record RefreshTokenRequest(long UserId, string DeviceUniqueId, string Platform, string RefreshToken);
        public record UpdateProfileRequest(string DisplayName, string? About, string? ProfilePhotoUrl);
    }


}

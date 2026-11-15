using System.Security.Cryptography;
using System.Text.RegularExpressions;
using WhatsAppClone.Api.Interface;  // IAuthManager yahan hai (apna namespace check kar lena)
using WhatsAppClone.Api.Models;
using WhatsAppClone.Api.Repositories;
using WhatsAppClone.Api.Services;

namespace WhatsAppClone.Api.Managers;

public partial class AuthManager(
    IOtpRepository otps,
    IUserRepository users,
    IDeviceRepository devices,
    ITokenService tokens,
    IConfiguration cfg) : IAuthManager
{
    [GeneratedRegex(@"^\+?[0-9]{10,15}$")]
    private static partial Regex PhoneRegex();

    private static string Normalize(string p)
    {
        var s = (p ?? "").Replace(" ", "").Replace("-", "");
        return s.StartsWith('+') ? s : "+" + s;
    }
    private string OtpHash(string phone, string otp) => tokens.Hash($"{phone}:{otp}");

    private static UserDto ToDto(User u) =>
        new(u.UserId, u.PhoneNumber, u.DisplayName, u.About, u.ProfilePhotoUrl);

    public async Task<ApiResponse<string>> SendOtpAsync(SendOtpRequest req)
    {
        var phone = Normalize(req.PhoneNumber);
        if (!PhoneRegex().IsMatch(phone))
            return ApiResponse<string>.Fail("Invalid phone number.");

        var otp = cfg.GetValue<bool>("Otp:UseFixed")
            ? "123456"
            : RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var r = await otps.CreateAsync(phone, OtpHash(phone, otp), DateTime.UtcNow.AddMinutes(5));
        if (r == -1)
            return ApiResponse<string>.Fail("Please wait 1 minute before requesting another OTP.");

        // TODO: SMS provider (MSG91 / Twilio) yahan otp bhejega
        return ApiResponse<string>.Ok("OTP sent.");
    }

    public async Task<ApiResponse<AuthResult>> VerifyOtpAsync(VerifyOtpRequest req)
    {
        var phone = Normalize(req.PhoneNumber);
        if (!PhoneRegex().IsMatch(phone) ||
            string.IsNullOrWhiteSpace(req.Otp) ||
            string.IsNullOrWhiteSpace(req.DeviceUniqueId))
            return ApiResponse<AuthResult>.Fail("Invalid request.");

        var code = await otps.VerifyAsync(phone, OtpHash(phone, req.Otp.Trim()));
        if (code != 1)
            return ApiResponse<AuthResult>.Fail(code switch
            {
                0 => "OTP not found. Request a new one.",
                -2 => "OTP expired.",
                -3 => "Too many wrong attempts. Request a new OTP.",
                _ => "Wrong OTP."
            });

        var user = await users.GetOrCreateByPhoneAsync(phone);
        if (!user.IsActive)
            return ApiResponse<AuthResult>.Fail("Account disabled.");

        var refresh = tokens.GenerateRefreshToken();
        await devices.UpsertAsync(user.UserId, req.DeviceUniqueId, req.Platform, req.PushToken,
            tokens.Hash(refresh), tokens.RefreshExpiry);

        return ApiResponse<AuthResult>.Ok(
            new(tokens.CreateAccessToken(user), refresh, ToDto(user), user.IsNewUser));
    }

    public async Task<ApiResponse<AuthResult>> RefreshAsync(RefreshTokenRequest req)
    {
        var user = await devices.ValidateRefreshTokenAsync(
            req.UserId, req.DeviceUniqueId, tokens.Hash(req.RefreshToken));
        if (user is null)
            return ApiResponse<AuthResult>.Fail("Invalid or expired refresh token.");

        // rotation: naya refresh token, purana invalid
        var refresh = tokens.GenerateRefreshToken();
        await devices.UpsertAsync(req.UserId, req.DeviceUniqueId, req.Platform, null,
            tokens.Hash(refresh), tokens.RefreshExpiry);

        return ApiResponse<AuthResult>.Ok(
            new(tokens.CreateAccessToken(user), refresh, ToDto(user), false));
    }

    public async Task<ApiResponse<string>> LogoutAsync(long userId, string deviceId)
    {
        await devices.RevokeAsync(userId, deviceId);
        return ApiResponse<string>.Ok("Logged out.");
    }
}
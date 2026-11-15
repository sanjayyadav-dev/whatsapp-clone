namespace WhatsAppClone.Api.Models;

public record SendOtpRequest(string PhoneNumber);
public record VerifyOtpRequest(string PhoneNumber, string Otp, string DeviceUniqueId, string Platform, string? PushToken);
public record RefreshTokenRequest(long UserId, string DeviceUniqueId, string Platform, string RefreshToken);
public record LogoutRequest(string DeviceUniqueId);

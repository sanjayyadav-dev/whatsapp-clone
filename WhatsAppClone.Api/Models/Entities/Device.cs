namespace WhatsAppClone.Api.Models;

// Table: dbo.Devices
public class Device
{
    public long DeviceId { get; set; }
    public long UserId { get; set; }
    public string DeviceUniqueId { get; set; } = "";
    public string Platform { get; set; } = "";
    public string? PushToken { get; set; }
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public DateTime? LastActiveAt { get; set; }
}

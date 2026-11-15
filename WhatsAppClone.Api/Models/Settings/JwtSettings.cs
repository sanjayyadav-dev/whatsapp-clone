namespace WhatsAppClone.Api.Models;

public class JwtSettings
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public int AccessMinutes { get; set; } = 15;
    public int RefreshDays { get; set; } = 30;
}

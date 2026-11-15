namespace WhatsAppClone.Api.Models;

// Table: dbo.OtpRequests
public class OtpRequest
{
    public long OtpId { get; set; }
    public string PhoneNumber { get; set; } = "";
    public string OtpHash { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public int AttemptCount { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}

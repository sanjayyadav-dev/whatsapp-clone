namespace WhatsAppClone.Api.Models;

// Table: dbo.Users
public class User
{
    public long UserId { get; set; }
    public string PhoneNumber { get; set; } = "";
    public string? DisplayName { get; set; }
    public string? About { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

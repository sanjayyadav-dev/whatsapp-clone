namespace WhatsAppClone.Api.Models;

public record UpdateProfileRequest(string DisplayName, string? About, string? ProfilePhotoUrl);

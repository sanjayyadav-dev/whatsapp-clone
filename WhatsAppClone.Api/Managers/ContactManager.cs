using System.Text.RegularExpressions;
using WhatsAppClone.Api.Interface;
using WhatsAppClone.Api.Models;
using WhatsAppClone.Api.Repositories;

namespace WhatsAppClone.Api.Managers;

public class ContactManager(IContactRepository contacts) : IContactManager
{
    private const int MaxPhonesPerSync = 500;

    private static readonly Regex PhoneRegex = new(@"^\+[0-9]{10,15}$", RegexOptions.Compiled);

    // "98765 43210", "09876543210", "0091 98765 43210" -> "+919876543210"
    private static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var hasPlus = raw.TrimStart().StartsWith('+');
        var digits = new string(raw.Where(char.IsDigit).ToArray());

        if (!hasPlus && digits.StartsWith("00")) { digits = digits[2..]; hasPlus = true; }
        if (!hasPlus && digits.Length == 11 && digits.StartsWith('0')) digits = digits[1..];   // 0 + 10 digit
        if (!hasPlus && digits.Length == 10) digits = "91" + digits;                           // India default

        var full = "+" + digits;
        return PhoneRegex.IsMatch(full) ? full : null;
    }

    private static UserDto ToDto(User u) =>
        new(u.UserId, u.PhoneNumber, u.DisplayName, u.About, u.ProfilePhotoUrl);

    public async Task<ApiResponse<List<UserDto>>> SyncAsync(long userId, ContactSyncRequest req)
    {
        if (req.PhoneNumbers is null || req.PhoneNumbers.Count == 0)
            return ApiResponse<List<UserDto>>.Ok([]);

        if (req.PhoneNumbers.Count > MaxPhonesPerSync)
            return ApiResponse<List<UserDto>>.Fail($"Max {MaxPhonesPerSync} numbers per request.");

        var phones = req.PhoneNumbers
            .Select(Normalize)
            .Where(p => p is not null)
            .Select(p => p!)
            .Distinct()
            .ToList();

        if (phones.Count == 0)
            return ApiResponse<List<UserDto>>.Ok([]);

        var found = await contacts.FindByPhonesAsync(phones, userId);
        return ApiResponse<List<UserDto>>.Ok(found.Select(ToDto).ToList());
    }
}
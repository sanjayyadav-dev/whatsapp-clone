using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WhatsAppClone.Api.Models;

namespace WhatsAppClone.Api.Services;

public interface ITokenService
{
    string CreateAccessToken(User user);
    string GenerateRefreshToken();
    DateTime RefreshExpiry { get; }
    string Hash(string value);
}

public class TokenService(IOptions<JwtSettings> options) : ITokenService
{
    private readonly JwtSettings _s = options.Value;

    public DateTime RefreshExpiry => DateTime.UtcNow.AddDays(_s.RefreshDays);

    public string CreateAccessToken(User user)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("uid", user.UserId.ToString()), new Claim("phone", user.PhoneNumber)]),
            Expires = DateTime.UtcNow.AddMinutes(_s.AccessMinutes),
            Issuer = _s.Issuer,
            Audience = _s.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_s.Key)), SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    // HMAC-SHA256 (key = Jwt:Key), OTP aur refresh token dono ke liye
    public string Hash(string value)
    {
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(_s.Key));
        return Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }
}
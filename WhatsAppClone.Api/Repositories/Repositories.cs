using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using WhatsAppClone.Api.Models;

namespace WhatsAppClone.Api.Repositories;

public interface IDbConnectionFactory { IDbConnection Create(); }

public class DbConnectionFactory(IConfiguration cfg) : IDbConnectionFactory
{
    public IDbConnection Create() => new SqlConnection(cfg.GetConnectionString("Default"));
}

public interface IOtpRepository
{
    Task<int> CreateAsync(string phone, string otpHash, DateTime expiresAt);
    Task<int> VerifyAsync(string phone, string otpHash);
}

public class OtpRepository(IDbConnectionFactory db) : IOtpRepository
{
    public async Task<int> CreateAsync(string phone, string otpHash, DateTime expiresAt)
    {
        using var c = db.Create();
        return await c.QuerySingleAsync<int>("dbo.usp_Otp_Create",
            new { PhoneNumber = phone, OtpHash = otpHash, ExpiresAt = expiresAt },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> VerifyAsync(string phone, string otpHash)
    {
        using var c = db.Create();
        return await c.QuerySingleAsync<int>("dbo.usp_Otp_Verify",
            new { PhoneNumber = phone, OtpHash = otpHash },
            commandType: CommandType.StoredProcedure);
    }
}

public interface IUserRepository
{
    Task<UserGetOrCreateResult> GetOrCreateByPhoneAsync(string phone);
    Task<User?> GetByIdAsync(long userId);
    Task<User?> UpdateProfileAsync(long userId, string displayName, string? about, string? photoUrl);
}

public class UserRepository(IDbConnectionFactory db) : IUserRepository
{
    public async Task<UserGetOrCreateResult> GetOrCreateByPhoneAsync(string phone)
    {
        using var c = db.Create();
        return await c.QuerySingleAsync<UserGetOrCreateResult>("dbo.usp_User_GetOrCreateByPhone",
            new { PhoneNumber = phone }, commandType: CommandType.StoredProcedure);
    }

    public async Task<User?> GetByIdAsync(long userId)
    {
        using var c = db.Create();
        return await c.QuerySingleOrDefaultAsync<User>("dbo.usp_User_GetById",
            new { UserId = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task<User?> UpdateProfileAsync(long userId, string displayName, string? about, string? photoUrl)
    {
        using var c = db.Create();
        return await c.QuerySingleOrDefaultAsync<User>("dbo.usp_User_UpdateProfile",
            new { UserId = userId, DisplayName = displayName, About = about, ProfilePhotoUrl = photoUrl },
            commandType: CommandType.StoredProcedure);
    }
}

public interface IDeviceRepository
{
    Task UpsertAsync(long userId, string deviceId, string platform, string? pushToken, string refreshHash, DateTime refreshExpiresAt);
    Task<User?> ValidateRefreshTokenAsync(long userId, string deviceId, string refreshHash);
    Task RevokeAsync(long userId, string deviceId);
}

public class DeviceRepository(IDbConnectionFactory db) : IDeviceRepository
{
    public async Task UpsertAsync(long userId, string deviceId, string platform, string? pushToken, string refreshHash, DateTime refreshExpiresAt)
    {
        using var c = db.Create();
        await c.ExecuteAsync("dbo.usp_Device_Upsert", new
        {
            UserId = userId,
            DeviceUniqueId = deviceId,
            Platform = platform,
            PushToken = pushToken,
            RefreshTokenHash = refreshHash,
            RefreshTokenExpiresAt = refreshExpiresAt
        }, commandType: CommandType.StoredProcedure);
    }

    public async Task<User?> ValidateRefreshTokenAsync(long userId, string deviceId, string refreshHash)
    {
        using var c = db.Create();
        return await c.QuerySingleOrDefaultAsync<User>("dbo.usp_Device_ValidateRefreshToken",
            new { UserId = userId, DeviceUniqueId = deviceId, RefreshTokenHash = refreshHash },
            commandType: CommandType.StoredProcedure);
    }

    public async Task RevokeAsync(long userId, string deviceId)
    {
        using var c = db.Create();
        await c.ExecuteAsync("dbo.usp_Device_Revoke",
            new { UserId = userId, DeviceUniqueId = deviceId }, commandType: CommandType.StoredProcedure);
    }
}
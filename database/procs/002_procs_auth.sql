/* Module 1 - Auth Stored Procedures
   DB: WhatsAppClone_Dev
   Repo path: database/procs/002_procs_auth.sql
   Result codes (OTP): 1=OK, 0=OTP not found, -1=rate limited, -2=expired, -3=too many attempts, -4=wrong OTP */
GO

CREATE OR ALTER PROCEDURE dbo.usp_Otp_Create
    @PhoneNumber NVARCHAR(20),
    @OtpHash     NVARCHAR(256),
    @ExpiresAt   DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    -- 1 OTP per minute per number
    IF EXISTS (SELECT 1 FROM dbo.OtpRequests
               WHERE PhoneNumber = @PhoneNumber
                 AND CreatedAt > DATEADD(SECOND, -60, SYSUTCDATETIME()))
    BEGIN
        SELECT -1 AS Result;
        RETURN;
    END

    -- purane unused OTP invalidate
    UPDATE dbo.OtpRequests SET IsUsed = 1
    WHERE PhoneNumber = @PhoneNumber AND IsUsed = 0;

    INSERT INTO dbo.OtpRequests (PhoneNumber, OtpHash, ExpiresAt, AttemptCount, IsUsed, CreatedAt)
    VALUES (@PhoneNumber, @OtpHash, @ExpiresAt, 0, 0, SYSUTCDATETIME());

    SELECT 1 AS Result;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Otp_Verify
    @PhoneNumber NVARCHAR(20),
    @OtpHash     NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OtpId BIGINT, @StoredHash NVARCHAR(256), @ExpiresAt DATETIME2, @Attempts INT;

    SELECT TOP 1 @OtpId = OtpId, @StoredHash = OtpHash, @ExpiresAt = ExpiresAt, @Attempts = AttemptCount
    FROM dbo.OtpRequests
    WHERE PhoneNumber = @PhoneNumber AND IsUsed = 0
    ORDER BY CreatedAt DESC;

    IF @OtpId IS NULL                    BEGIN SELECT 0  AS Result; RETURN; END
    IF @ExpiresAt < SYSUTCDATETIME()     BEGIN SELECT -2 AS Result; RETURN; END
    IF @Attempts >= 5                    BEGIN SELECT -3 AS Result; RETURN; END

    IF @StoredHash <> @OtpHash
    BEGIN
        UPDATE dbo.OtpRequests SET AttemptCount = AttemptCount + 1 WHERE OtpId = @OtpId;
        SELECT -4 AS Result;
        RETURN;
    END

    UPDATE dbo.OtpRequests SET IsUsed = 1 WHERE OtpId = @OtpId;
    SELECT 1 AS Result;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetOrCreateByPhone
    @PhoneNumber NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IsNew BIT = 0;

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE PhoneNumber = @PhoneNumber)
    BEGIN
        INSERT INTO dbo.Users (PhoneNumber, IsActive, CreatedAt, UpdatedAt)
        VALUES (@PhoneNumber, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
        SET @IsNew = 1;
    END

    SELECT UserId, PhoneNumber, DisplayName, About, ProfilePhotoUrl, IsActive, @IsNew AS IsNewUser
    FROM dbo.Users
    WHERE PhoneNumber = @PhoneNumber;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_UpdateProfile
    @UserId          BIGINT,
    @DisplayName     NVARCHAR(100),
    @About           NVARCHAR(200) = NULL,
    @ProfilePhotoUrl NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET DisplayName     = @DisplayName,
        About           = @About,
        ProfilePhotoUrl = @ProfilePhotoUrl,
        UpdatedAt       = SYSUTCDATETIME()
    WHERE UserId = @UserId;

    SELECT UserId, PhoneNumber, DisplayName, About, ProfilePhotoUrl, IsActive
    FROM dbo.Users WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Device_Upsert
    @UserId                BIGINT,
    @DeviceUniqueId        NVARCHAR(200),
    @Platform              NVARCHAR(20),
    @PushToken             NVARCHAR(500) = NULL,
    @RefreshTokenHash      NVARCHAR(256),
    @RefreshTokenExpiresAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Devices
    SET Platform = @Platform,
        PushToken = ISNULL(@PushToken, PushToken),
        RefreshTokenHash = @RefreshTokenHash,
        RefreshTokenExpiresAt = @RefreshTokenExpiresAt,
        LastActiveAt = SYSUTCDATETIME()
    WHERE UserId = @UserId AND DeviceUniqueId = @DeviceUniqueId;

    IF @@ROWCOUNT = 0
        INSERT INTO dbo.Devices (UserId, DeviceUniqueId, Platform, PushToken,
                                 RefreshTokenHash, RefreshTokenExpiresAt, LastActiveAt)
        VALUES (@UserId, @DeviceUniqueId, @Platform, @PushToken,
                @RefreshTokenHash, @RefreshTokenExpiresAt, SYSUTCDATETIME());
END
GO

-- Valid ho to user row return karta hai, warna koi row nahi
CREATE OR ALTER PROCEDURE dbo.usp_Device_ValidateRefreshToken
    @UserId           BIGINT,
    @DeviceUniqueId   NVARCHAR(200),
    @RefreshTokenHash NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT u.UserId, u.PhoneNumber, u.DisplayName
    FROM dbo.Devices d
    JOIN dbo.Users u ON u.UserId = d.UserId
    WHERE d.UserId = @UserId
      AND d.DeviceUniqueId = @DeviceUniqueId
      AND d.RefreshTokenHash = @RefreshTokenHash
      AND d.RefreshTokenExpiresAt > SYSUTCDATETIME()
      AND u.IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Device_Revoke
    @UserId         BIGINT,
    @DeviceUniqueId NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Devices
    SET RefreshTokenHash = NULL, RefreshTokenExpiresAt = NULL, PushToken = NULL
    WHERE UserId = @UserId AND DeviceUniqueId = @DeviceUniqueId;
END
GO

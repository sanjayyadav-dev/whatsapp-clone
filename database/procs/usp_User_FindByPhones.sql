CREATE OR ALTER PROCEDURE dbo.usp_User_FindByPhones
    @PhonesJson    NVARCHAR(MAX),   -- ["+919876543210","+911234567890"]
    @ExcludeUserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT u.UserId, u.PhoneNumber, u.DisplayName, u.About, u.ProfilePhotoUrl
    FROM dbo.Users u
    JOIN (
        SELECT DISTINCT PhoneNumber
        FROM OPENJSON(@PhonesJson) WITH (PhoneNumber NVARCHAR(20) '$')
    ) p ON p.PhoneNumber = u.PhoneNumber
    WHERE u.IsActive = 1
      AND u.UserId <> @ExcludeUserId;
END
GO

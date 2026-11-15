CREATE OR ALTER PROCEDURE dbo.usp_User_GetById
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId, PhoneNumber, DisplayName, About, ProfilePhotoUrl, IsActive
    FROM dbo.Users WHERE UserId = @UserId AND IsActive = 1;
END
GO

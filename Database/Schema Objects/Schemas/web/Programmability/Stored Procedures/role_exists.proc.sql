
CREATE PROCEDURE [web].[role_exists]
    @role_name         nvarchar(256)
AS
BEGIN
    IF (EXISTS (SELECT name FROM dbo.roles WHERE LOWER(@role_name) = LOWER(name)))
        RETURN(1)
    ELSE
        RETURN(0)
END
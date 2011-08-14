
CREATE PROCEDURE [web].[users_in_roles_get_roles_for_user]
    @email         nvarchar(256)
AS
BEGIN
    DECLARE @UserId int
    SELECT  @UserId = NULL

    SELECT  @UserId = id
		FROM    dbo.users
			WHERE   LOWER(email) = LOWER(@email)

    IF (@UserId IS NULL)
        RETURN(1)

    SELECT r.name
		FROM dbo.roles r, dbo.users_in_roles ur
			WHERE  r.id = ur.id_role AND ur.id_user = @UserId
			ORDER BY r.name
    RETURN (0)
END
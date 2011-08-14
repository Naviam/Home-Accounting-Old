CREATE PROCEDURE [web].[users_in_roles_is_user_in_role]
    @email			nvarchar(256),
    @role_name      nvarchar(256)
AS
BEGIN
    DECLARE @UserId int
    SELECT  @UserId = NULL
    DECLARE @RoleId int
    SELECT  @RoleId = NULL

    SELECT  @UserId = id
		FROM    dbo.users
			WHERE   LOWER(email) = LOWER(@email)

    IF (@UserId IS NULL)
        RETURN(2)

    SELECT  @RoleId = id
		FROM    dbo.roles
			WHERE   LOWER(name) = LOWER(@role_name)

    IF (@RoleId IS NULL)
        RETURN(3)

    IF (EXISTS( SELECT * FROM dbo.users_in_roles WHERE  id_user = @UserId AND id_role = @RoleId))
        RETURN(1)
    ELSE
        RETURN(0)
END
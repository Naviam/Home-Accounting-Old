CREATE PROCEDURE [web].[users_in_roles_get_users_in_role]
    @role_name         nvarchar(256)
AS
BEGIN
     DECLARE @RoleId int
     SELECT  @RoleId = NULL

     SELECT  @RoleId = id
     FROM    dbo.roles
     WHERE   LOWER(@role_name) = LOWER(name)
     IF (@RoleId IS NULL)
         RETURN(1)

    SELECT u.email
		FROM   dbo.users u, dbo.users_in_roles ur
			WHERE  u.id = ur.id_user AND @RoleId = ur.id_role
    ORDER BY u.email
    RETURN(0)
END
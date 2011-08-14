
CREATE PROCEDURE [web].[users_in_roles_find_users_in_role]
    @role_name				nvarchar(256),
    @user_name_to_match		nvarchar(256)
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
			WHERE  u.id = ur.id_user AND @RoleId = ur.id_role AND u.email LIKE LOWER(@user_name_to_match)
			ORDER BY u.email
    RETURN(0)
END
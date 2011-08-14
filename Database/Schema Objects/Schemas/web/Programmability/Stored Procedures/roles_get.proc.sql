
CREATE PROCEDURE [web].[roles_get] 
AS
BEGIN
    SELECT name
		FROM   dbo.roles
			ORDER BY name
END
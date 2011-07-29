-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Get user
-- =============================================
CREATE PROCEDURE [web].[get_user]
	@email nvarchar(50)
AS
BEGIN
  SELECT * FROM users WHERE users.email = @email
END
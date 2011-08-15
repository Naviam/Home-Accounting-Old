-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 04/08/2011
-- Description:	Get user
-- =============================================
CREATE PROCEDURE [web].[companies_get]
	@id_user int
AS
BEGIN
  SELECT c.id, c.id_country, c.name_business, c.id_company_type 
	FROM users_companies us 
		 inner join companies c ON c.id = us.id_company 
	WHERE id_user = @id_user
END
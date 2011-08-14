-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Delete category
-- =============================================
CREATE PROCEDURE [web].[category_delete]
	@id int
AS
BEGIN
	DELETE FROM categories WHERE id = @id and id_user is not null
END
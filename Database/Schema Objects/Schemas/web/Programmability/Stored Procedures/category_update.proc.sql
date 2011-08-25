-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 23/08/2011
-- Description:	Update category
-- =============================================
CREATE PROCEDURE [web].[category_update]
	@id int,
	@id_user int,
	@parent_id int,
	@name nvarchar(50)
AS
BEGIN
    IF (NOT EXISTS( SELECT TOP 1 id FROM categories WHERE id_user = @id_user and id = @id))
        RETURN(1)
        
	UPDATE [dbo].[categories] 
		SET  [name] = @name
	WHERE [id]=@id and id_user = @id_user
END
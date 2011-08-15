-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Create category
-- =============================================
CREATE PROCEDURE [web].[category_create]
	@id_user int,
	@parent_id int,
	@name nvarchar(50),
	@id int out
AS
BEGIN
	INSERT INTO [dbo].[categories]
           ([name],[parent_id],[id_user])
     VALUES
           (@name, @parent_id, @id_user);
           
    SET @id = SCOPE_IDENTITY();
END
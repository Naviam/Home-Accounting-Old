-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Add category
-- =============================================
CREATE PROCEDURE [web].[add_category]
	@id_user int,
	@parent_id int,
	@name nvarchar(50),
	@id int out
AS
BEGIN
	INSERT INTO [naviam].[dbo].[categories]
           ([name],[parent_id],[id_user])
     VALUES
           (@name, @parent_id, @name);
           
    SET @id = SCOPE_IDENTITY();
END
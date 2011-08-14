-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Get categories
-- =============================================
CREATE PROCEDURE [web].[categories_get]
	@id_user int = NULL
AS
BEGIN
 SET NOCOUNT ON;
 
 WITH tree (id, parent_id, name, id_user, level) as 
(
   SELECT id, parent_id, name, id_user, 0 as level 
   FROM categories
   WHERE parent_id is null

   UNION ALL

   SELECT c2.id, c2.parent_id, c2.name, c2.id_user, level+1
   FROM categories c2 
     INNER JOIN tree ON tree.id = c2.parent_id
)
SELECT * FROM tree 
	WHERE id_user is NULL OR id_user = @id_user 
		ORDER BY level 
END
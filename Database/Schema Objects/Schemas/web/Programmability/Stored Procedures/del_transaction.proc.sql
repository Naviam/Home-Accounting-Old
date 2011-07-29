-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Delete transaction
-- =============================================
CREATE PROCEDURE [web].[del_transaction]
	@id int
AS
BEGIN
	DELETE FROM dbo.transactions WHERE id = @id
END
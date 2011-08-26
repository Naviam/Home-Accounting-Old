-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Delete transaction
-- =============================================
CREATE PROCEDURE [web].[transaction_delete]
	@id int,
	@id_company int
AS
BEGIN
    IF (NOT EXISTS( SELECT TOP 1 t.id FROM transactions t 
					INNER JOIN accounts a ON t.id =@id AND
											 a.id = t.id_account AND
											 a.id_company = @id_company ))
        RETURN(1)
        
	DELETE FROM dbo.transactions WHERE id = @id
END
-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 14/08/2011
-- Description:	Delete account
-- =============================================
CREATE PROCEDURE [web].[accounts_delete]
	@id	int,
	@id_company int
AS
BEGIN
    IF (NOT EXISTS( SELECT TOP 1 id FROM accounts WHERE id_company = @id_company and id = @id))
        RETURN(1)
	--TODO: check if last account(need 1 or more)
	--TODO: move transactions to history
	
	--delete transactions
	DELETE FROM transactions WHERE id_account = @id;
	
	--delete account
	DELETE FROM accounts WHERE id = @id;
	
	RETURN(0)
END
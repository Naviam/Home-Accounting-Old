-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 27/08/2011
-- Description:	Add money to account
-- =============================================
CREATE PROCEDURE [web].[account_add_amount]
	@id_company int,
	@id_account int,
	@amount_value money
AS
BEGIN
	DECLARE @balance money = null;
	--get current balance
	SELECT @balance = balance FROM accounts WHERE id_company = @id_company and id = @id_account
	--update balance
	UPDATE accounts 
		SET  balance = @balance + @amount_value
			WHERE id_company = @id_company and id = @id_account
END
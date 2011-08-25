-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 14/08/2011
-- Description:	Get accounts
-- =============================================
CREATE PROCEDURE [web].[accounts_get]
	@id_company int,
	@id_account int = null
AS
BEGIN
	SET NOCOUNT ON;

	SELECT a.id, a.number, a.date_creation, a.balance, a.[description], a.id_company,
		   a.id_currency, a.id_type, a.initial_balance  
		FROM accounts a
			WHERE (@id_account is NULL AND a.id_company = @id_company) OR 
				  (@id_account is not NULL AND a.id = @id_account AND a.id_company = @id_company)
END
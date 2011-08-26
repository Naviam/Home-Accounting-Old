-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Get transactions
-- =============================================
CREATE PROCEDURE [web].[transactions_get]
	@id_company int,
	@id_transaction int = null
AS
BEGIN
	SELECT trn.id, trn.[date], trn.amount, trn.merchant, trn.[description], trn.notes,
		   trn.[type] as transaction_type, trn.direction, trn.id_account, 
		   trn.id_category, acc.[id_currency] as [id_currency], trn.include_in_tax
		FROM companies cmp 
			 inner join accounts acc ON  acc.id_company = cmp.id
			 inner join transactions trn ON trn.id_account = acc.id
		WHERE (@id_transaction is NULL AND cmp.id = @id_company) OR 
			  (@id_transaction is not NULL AND trn.id = @id_transaction)
END
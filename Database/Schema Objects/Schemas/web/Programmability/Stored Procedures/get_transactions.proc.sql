-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Get transactions
-- =============================================
CREATE PROCEDURE [web].[get_transactions]
	@id_company int
AS
BEGIN
	SELECT trn.id, trn.[date], trn.amount, trn.merchant, trn.[description], trn.notes,
		   trn.[type] as transaction_type, trn.direction, trn.id_account, 
		   acc.number as account_number, acc.[type] as account_type, ctg.name
		FROM companies cmp 
			 inner join accounts acc ON  acc.id_company = cmp.id
			 inner join transactions trn ON trn.id_account = acc.id
			 left join categories ctg ON trn.id_category = ctg.id
		WHERE cmp.id = @id_company
END
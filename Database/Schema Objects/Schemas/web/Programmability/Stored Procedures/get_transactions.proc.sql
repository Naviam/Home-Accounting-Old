-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Get transactions
-- =============================================
CREATE PROCEDURE [web].[get_transactions]
	@id_company int,
	@id_transaction int = null,
	@id_language int = null --rus
AS
BEGIN
IF(@id_language is null) SET @id_language = 1;
	SELECT trn.id, trn.[date], trn.amount, trn.merchant, trn.[description], trn.notes,
		   trn.[type] as transaction_type, trn.direction, trn.id_account, 
		   acc.number as account_number, atl.[type_name] as account_type, ctg.id as category_id, ctg.name as category_name
		FROM companies cmp 
			 inner join accounts acc ON  acc.id_company = cmp.id
			 inner join account_types at ON at.id = acc.id_type
			 inner join account_types_lng atl ON atl.id_account_type = at.id AND atl.id_language = @id_language
			 inner join transactions trn ON trn.id_account = acc.id
			 left join categories ctg ON trn.id_category = ctg.id
		WHERE (@id_transaction is NULL AND cmp.id = @id_company) OR 
			  (@id_transaction is not NULL AND trn.id = @id_transaction)
END
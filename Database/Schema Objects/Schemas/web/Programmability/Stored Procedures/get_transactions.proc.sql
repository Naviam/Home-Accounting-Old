-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Get transactions
-- =============================================
CREATE PROCEDURE [web].[get_transactions]
	@id_company int
AS
BEGIN
	SELECT * 
		FROM companies cmp 
			 inner join accounts acc ON  acc.id_company = cmp.id
			 inner join transactions trn ON trn.id_account = acc.id
		WHERE cmp.id = @id_company
END
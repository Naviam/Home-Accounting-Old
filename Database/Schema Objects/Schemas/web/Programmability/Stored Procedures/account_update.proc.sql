-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 22/08/2011
-- Description:	Update transaction
-- =============================================
CREATE PROCEDURE [web].[account_update]
	@id							int,
	@curr_date_utc				datetime,
	@name						nvarchar(50),
	@id_company					int,
	@id_currency				int,
	@balance					money,
	@id_type					int,
	@description				nvarchar(500),
	@initial_balance			money,
	@id_financial_institution	int,
	@card_number				nvarchar(20)
AS
BEGIN
	UPDATE [dbo].[accounts] 
		SET  [name] = @name
			,[id_currency] = @id_currency
			,[balance] = @balance
			,[id_type] = @id_type
			,[description] = @description
			,[initial_balance] = @initial_balance
			,[id_financial_institution] = @id_financial_institution
			,[card_number] = @card_number
	WHERE [id]=@id
END
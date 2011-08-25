-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 22/08/2011
-- Description:	Update transaction
-- =============================================
CREATE PROCEDURE [web].[account_update]
	@id int,
	@curr_date_utc datetime,
	@number nvarchar(50),
	@id_company int,
	@id_currency int,
	@balance money,
	@id_type int,
	@description nvarchar(500),
	@initial_balance money
AS
BEGIN
	UPDATE [dbo].[accounts] 
		SET  [number] = @number
			,[id_currency] = @id_currency
			,[balance] = @balance
			,[id_type] = @id_type
			,[description] = @description
			,[initial_balance] = @initial_balance
	WHERE [id]=@id
END
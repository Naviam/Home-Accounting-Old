﻿-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Update transaction
-- =============================================
CREATE PROCEDURE [web].[transaction_update]
	@id int,
	@date datetime,
	@amount money,
	@merchant nvarchar(50),
	@id_account int,
	@id_category int,
	@description nvarchar(100),
	@notes nvarchar(1000),
	@type int, --can be: 0-cash, 1-check, 2-pending
	@direction int --can be: 0-expense, 1-income
	
AS
BEGIN
	UPDATE [dbo].[transactions] 
		SET  [date] = @date
			,[amount] = @amount
			,[merchant] = @merchant
			,[id_account] = @id_account
			,[id_category] = @id_category
			,[description] = @description
			,[notes] = @notes
			,[type] = @type
			,[direction] = @direction
	WHERE [id]=@id
END
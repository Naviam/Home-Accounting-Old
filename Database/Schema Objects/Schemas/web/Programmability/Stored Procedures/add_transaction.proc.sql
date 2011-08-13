-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Add transaction
-- =============================================
CREATE PROCEDURE [web].[add_transaction]
	@id int out,
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
	INSERT [dbo].[transactions] 
			([date], [amount], [merchant], [id_account], [id_category], 
			 [description], [notes], [type], [direction]) 
	VALUES  (@date, @amount, @merchant, @id_account, @id_category,
			 @description, @notes, @type, @direction)
	SET @id = SCOPE_IDENTITY();
END
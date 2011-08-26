-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Create transaction
-- =============================================
CREATE PROCEDURE [web].[transaction_create]
	@id int out,
	@date datetime,
	@amount money,
	@merchant nvarchar(50),
	@id_account int,
	@id_category int,
	@description nvarchar(100),
	@notes nvarchar(1000),
	@type int, --can be: 0-cash, 1-check, 2-pending
	@direction int, --can be: 0-expense, 1-income
	@include_in_tax bit
	
AS
BEGIN
	INSERT [dbo].[transactions] 
			([date], [amount], [merchant], [id_account], [id_category], 
			 [description], [notes], [type], [direction],[include_in_tax]) 
	VALUES  (@date, @amount, @merchant, @id_account, @id_category,
			 @description, @notes, @type, @direction, @include_in_tax)
	SET @id = SCOPE_IDENTITY();
END
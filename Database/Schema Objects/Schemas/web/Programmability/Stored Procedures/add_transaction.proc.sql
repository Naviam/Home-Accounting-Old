-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Add transaction
-- =============================================
CREATE PROCEDURE [web].[add_transaction]
	@date datetime,
	@amount money,
	@merchant nvarchar(50),
	@id_account int,
	@id_category int,
	@description nvarchar(100),
	@notes nvarchar(1000),
	@type nvarchar(50), --can be: cash, check, pending
	@direction nvarchar(10),--can be: expense, income
	@id int out 
AS
BEGIN
	INSERT [dbo].[transactions] 
			([date], [amount], [merchant], [id_account], [id_category], 
			 [description], [notes], [type], [direction]) 
	VALUES  (@date, @amount, @merchant, @id_account, @id_category,
			 @description, @notes, @type, @direction)
	SET @id = SCOPE_IDENTITY();
END
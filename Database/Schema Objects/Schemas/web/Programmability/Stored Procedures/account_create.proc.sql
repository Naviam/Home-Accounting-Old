-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 14/08/2011
-- Description:	Create account
-- =============================================
CREATE  PROCEDURE [web].[account_create]
	@curr_date_utc		datetime,
	@number				nvarchar(50),
	@id_company			int,	
	@id_currency		int,
	@balance			money,
	@id_type			int,
	@description		nvarchar(500),
	@initial_balance	money,
	@id					int OUT
AS
BEGIN
	INSERT [dbo].[accounts] 
			(date_creation, number, id_company, id_currency, balance, id_type, [description], initial_balance) 
	VALUES  (@curr_date_utc, @number, @id_company, @id_currency, @balance, @id_type, @description, @initial_balance )
	SET @id = SCOPE_IDENTITY();
END
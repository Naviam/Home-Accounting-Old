-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 14/08/2011
-- Description:	Create account
-- =============================================
CREATE  PROCEDURE [web].[account_create]
	@curr_date_utc				datetime,
	@name						nvarchar(50),
	@id_company					int,	
	@id_currency				int,
	@balance					money,
	@id_type					int,
	@description				nvarchar(500),
	@initial_balance			money,
	@id_financial_institution	int,
	@card_number				nvarchar(20),
	@id							int OUT
AS
BEGIN
	INSERT [dbo].[accounts] 
			(date_creation, name, id_company, id_currency, balance, id_type, 
			[description], initial_balance, id_financial_institution, card_number) 
	VALUES  (@curr_date_utc, @name, @id_company, @id_currency, @balance, @id_type, 
			@description, @initial_balance, @id_financial_institution, @card_number )
	SET @id = SCOPE_IDENTITY();
END
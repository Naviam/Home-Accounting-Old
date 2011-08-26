-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 23/08/2011
-- Description:	Calculate Balance
-- =============================================
CREATE FUNCTION [web].[CalculateBalance] 
(
	@id_account int
)
RETURNS money
AS
BEGIN
	DECLARE @Result money

	SELECT @Result = SUM(amount*CASE direction WHEN 0 THEN -1 ELSE 1 END) 
		FROM transactions WHERE id_account = @id_account
	
	SELECT @Result = @Result + ISNUll(initial_balance,0) FROM accounts WHERE id = @id_account

	RETURN @Result
END
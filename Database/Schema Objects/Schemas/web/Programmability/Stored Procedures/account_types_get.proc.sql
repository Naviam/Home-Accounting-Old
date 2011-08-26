-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 18/08/2011
-- Description:	Get account types
-- =============================================
CREATE PROCEDURE [web].[account_types_get]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT id, [type_name], [type_description]  
		FROM account_types
END
-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 30/08/2011
-- Description:	Get financial institutions
-- =============================================
CREATE PROCEDURE [web].[financial_institutions_get]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT id, name, name_short, [description], id_type FROM financial_institutions
END
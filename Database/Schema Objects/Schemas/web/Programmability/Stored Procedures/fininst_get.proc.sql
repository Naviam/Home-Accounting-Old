-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 18/08/2011
-- Description:	Get account types
-- =============================================
CREATE PROCEDURE [web].[fininst_get]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT id, [name], [description], [id_type]
		FROM financial_institutions
END
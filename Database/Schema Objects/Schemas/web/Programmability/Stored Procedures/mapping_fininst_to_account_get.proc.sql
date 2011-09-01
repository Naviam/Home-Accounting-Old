-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 18/08/2011
-- Description:	Get account types
-- =============================================
CREATE PROCEDURE [web].[mapping_fininst_to_account_get]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT [id_fin_type], [id_acc_type]
		FROM mapping_fininst_account_types
END
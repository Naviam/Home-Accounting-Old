-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Get user
-- =============================================
CREATE PROCEDURE [web].[currencies_get]
AS
BEGIN
  SELECT id, name_short	FROM currencies
END
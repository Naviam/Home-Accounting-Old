CREATE TABLE [dbo].[exchange_rates] (
    [id]          INT      IDENTITY (1, 1) NOT NULL,
    [id_country]  INT      NOT NULL,
    [value]       MONEY    NOT NULL,
    [date]        DATETIME NOT NULL,
    [id_currency] INT      NOT NULL
);


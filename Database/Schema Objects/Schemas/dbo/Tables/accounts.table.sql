CREATE TABLE [dbo].[accounts] (
    [id]                       INT            IDENTITY (1, 1) NOT NULL,
    [date_creation]            DATETIME       NOT NULL,
    [name]                     NVARCHAR (50)  COLLATE Latin1_General_CS_AS NOT NULL,
    [id_company]               INT            NOT NULL,
    [id_currency]              INT            NOT NULL,
    [balance]                  MONEY          NOT NULL,
    [id_type]                  INT            NOT NULL,
    [description]              NVARCHAR (500) NULL,
    [initial_balance]          MONEY          NOT NULL,
    [id_financial_institution] INT            NULL,
    [card_number]              NVARCHAR (20)  NULL
);








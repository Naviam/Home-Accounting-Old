CREATE TABLE [dbo].[users_currencies] (
    [id]              INT      IDENTITY (1, 1) NOT NULL,
    [id_user]         INT      NOT NULL,
    [id_curency_from] INT      NOT NULL,
    [id_currency_to]  INT      NOT NULL,
    [value]           MONEY    NOT NULL,
    [date]            DATETIME NOT NULL
);


CREATE TABLE [dbo].[accounts] (
    [id]            INT           IDENTITY (1, 1) NOT NULL,
    [date_creation] DATETIME      NOT NULL,
    [number]        NVARCHAR (50) COLLATE Latin1_General_CS_AS NOT NULL,
    [id_company]    INT           NOT NULL,
    [type]          NVARCHAR (20) NOT NULL,
    [id_currency]   INT           NULL
);


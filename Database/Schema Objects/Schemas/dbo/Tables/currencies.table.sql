CREATE TABLE [dbo].[currencies] (
    [id]         INT           IDENTITY (1, 1) NOT NULL,
    [code]       NVARCHAR (5)  NULL,
    [name_short] NVARCHAR (10) COLLATE Latin1_General_CS_AS NULL,
    [name]       NVARCHAR (50) NULL
);




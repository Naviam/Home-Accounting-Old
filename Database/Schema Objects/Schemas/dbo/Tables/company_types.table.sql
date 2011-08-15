CREATE TABLE [dbo].[company_types] (
    [id]          INT            IDENTITY (1, 1) NOT NULL,
    [name]        NVARCHAR (50)  COLLATE Latin1_General_CS_AS NULL,
    [name_short]  NVARCHAR (10)  NULL,
    [description] NVARCHAR (500) NULL
);


CREATE TABLE [dbo].[financial_institutions] (
    [id]          INT            IDENTITY (1, 1) NOT NULL,
    [name]        NVARCHAR (50)  COLLATE Latin1_General_CS_AS NOT NULL,
    [name_short]  NVARCHAR (10)  COLLATE Latin1_General_CS_AS NULL,
    [description] NVARCHAR (500) NULL,
    [id_type]     INT            NULL
);




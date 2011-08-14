CREATE TABLE [dbo].[countries] (
    [id]         INT           IDENTITY (1, 1) NOT NULL,
    [name]       NVARCHAR (50) COLLATE Latin1_General_CS_AS NOT NULL,
    [short_name] NVARCHAR (10) COLLATE Latin1_General_CS_AS NOT NULL
);


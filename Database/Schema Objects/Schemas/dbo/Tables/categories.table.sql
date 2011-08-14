CREATE TABLE [dbo].[categories] (
    [id]        INT           IDENTITY (1, 1) NOT NULL,
    [name]      NVARCHAR (50) COLLATE Latin1_General_CS_AS NULL,
    [parent_id] INT           NULL,
    [id_user]   INT           NULL
);


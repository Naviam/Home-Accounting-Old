CREATE TABLE [dbo].[users] (
    [id]         INT            IDENTITY (1, 1) NOT NULL,
    [email]      NVARCHAR (50)  COLLATE Latin1_General_CS_AS NOT NULL,
    [password]   NVARCHAR (200) COLLATE Latin1_General_CS_AS NOT NULL,
    [first_name] NVARCHAR (50)  NULL,
    [last_name]  NVARCHAR (50)  NULL
);




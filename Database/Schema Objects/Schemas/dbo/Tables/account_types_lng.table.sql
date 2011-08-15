CREATE TABLE [dbo].[account_types_lng] (
    [id]               INT            IDENTITY (1, 1) NOT NULL,
    [id_account_type]  INT            NOT NULL,
    [id_language]      INT            NOT NULL,
    [type_name]        NVARCHAR (50)  NOT NULL,
    [type_description] NVARCHAR (500) NOT NULL
);


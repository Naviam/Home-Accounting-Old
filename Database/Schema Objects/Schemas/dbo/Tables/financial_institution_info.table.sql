CREATE TABLE [dbo].[financial_institution_info] (
    [id]          INT            IDENTITY (1, 1) NOT NULL,
    [id_fin]      INT            NULL,
    [id_country]  INT            NULL,
    [site]        NVARCHAR (100) NULL,
    [address]     NVARCHAR (200) NULL,
    [description] NVARCHAR (500) NULL,
    [email]       NVARCHAR (50)  NULL,
    [phone]       NVARCHAR (50)  NULL,
    [telex]       NVARCHAR (50)  NULL,
    [fax]         NVARCHAR (50)  NULL
);


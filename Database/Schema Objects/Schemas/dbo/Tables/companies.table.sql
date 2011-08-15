CREATE TABLE [dbo].[companies] (
    [id]              INT           IDENTITY (1, 1) NOT NULL,
    [id_country]      INT           NOT NULL,
    [name_business]   NVARCHAR (50) COLLATE Latin1_General_CS_AS NULL,
    [id_company_type] INT           NOT NULL
);


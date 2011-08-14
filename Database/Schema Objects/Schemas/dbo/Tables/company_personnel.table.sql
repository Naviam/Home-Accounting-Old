CREATE TABLE [dbo].[company_personnel] (
    [id]                 INT           IDENTITY (1, 1) NOT NULL,
    [job_title]          NVARCHAR (50) COLLATE Latin1_General_CS_AS NULL,
    [name]               NVARCHAR (50) COLLATE Latin1_General_CS_AS NULL,
    [date_created]       DATETIME      NOT NULL,
    [phone]              NVARCHAR (50) COLLATE Latin1_General_CS_AS NULL,
    [mobile]             NVARCHAR (50) COLLATE Latin1_General_CS_AS NULL,
    [email]              NVARCHAR (50) COLLATE Latin1_General_CS_AS NULL,
    [id_company]         INT           NOT NULL,
    [is_primory_contact] BIT           NOT NULL
);


CREATE TABLE [dbo].[transactions] (
    [id]          INT             IDENTITY (1, 1) NOT NULL,
    [date]        DATETIME        NOT NULL,
    [amount]      MONEY           NULL,
    [merchant]    NVARCHAR (50)   COLLATE Latin1_General_CS_AS NULL,
    [id_account]  INT             NOT NULL,
    [id_category] INT             NOT NULL,
    [description] NVARCHAR (100)  COLLATE Latin1_General_CS_AS NOT NULL,
    [notes]       NVARCHAR (1000) COLLATE Latin1_General_CS_AS NULL,
    [type]        INT             NOT NULL,
    [direction]   INT             NOT NULL
);




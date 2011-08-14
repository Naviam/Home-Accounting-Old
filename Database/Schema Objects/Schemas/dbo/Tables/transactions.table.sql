CREATE TABLE [dbo].[transactions] (
    [id]          INT             IDENTITY (1, 1) NOT NULL,
    [date]        DATETIME        NOT NULL,
    [amount]      MONEY           NULL,
    [merchant]    NVARCHAR (50)   COLLATE Latin1_General_CS_AS NULL,
    [id_account]  INT             NOT NULL,
    [id_category] INT             NOT NULL,
    [description] NVARCHAR (100)  COLLATE Latin1_General_CS_AS NULL,
    [notes]       NVARCHAR (1000) COLLATE Latin1_General_CS_AS NULL,
    [type]        INT             NOT NULL,
    [direction]   INT             NOT NULL
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'can be: 0 - cash, 1 - check,  2 - pending', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'transactions', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'can be: 0 - expense, 1 - income', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'transactions', @level2type = N'COLUMN', @level2name = N'direction';


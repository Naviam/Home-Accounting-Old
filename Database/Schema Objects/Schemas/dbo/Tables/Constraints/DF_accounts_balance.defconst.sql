ALTER TABLE [dbo].[accounts]
    ADD CONSTRAINT [DF_accounts_balance] DEFAULT ((0)) FOR [balance];


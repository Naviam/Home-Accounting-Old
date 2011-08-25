ALTER TABLE [dbo].[accounts]
    ADD CONSTRAINT [DF_accounts_initial_balance] DEFAULT ((0)) FOR [initial_balance];


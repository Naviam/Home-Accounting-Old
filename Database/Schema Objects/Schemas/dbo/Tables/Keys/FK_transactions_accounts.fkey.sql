ALTER TABLE [dbo].[transactions]
    ADD CONSTRAINT [FK_transactions_accounts] FOREIGN KEY ([id_account]) REFERENCES [dbo].[accounts] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


ALTER TABLE [dbo].[accounts]
    ADD CONSTRAINT [FK_accounts_account_types] FOREIGN KEY ([id_type]) REFERENCES [dbo].[account_types] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


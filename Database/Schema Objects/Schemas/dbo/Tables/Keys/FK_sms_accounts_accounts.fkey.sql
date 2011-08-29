ALTER TABLE [dbo].[sms_accounts]
    ADD CONSTRAINT [FK_sms_accounts_accounts] FOREIGN KEY ([id_account]) REFERENCES [dbo].[accounts] ([id]) ON DELETE CASCADE ON UPDATE NO ACTION;


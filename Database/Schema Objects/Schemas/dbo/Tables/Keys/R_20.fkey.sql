ALTER TABLE [dbo].[bank_accounts]
    ADD CONSTRAINT [R_20] FOREIGN KEY ([id_account]) REFERENCES [dbo].[accounts] ([id]) ON DELETE CASCADE ON UPDATE NO ACTION;


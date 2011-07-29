ALTER TABLE [dbo].[bank_accounts]
    ADD CONSTRAINT [R_21] FOREIGN KEY ([id_bank]) REFERENCES [dbo].[banks] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


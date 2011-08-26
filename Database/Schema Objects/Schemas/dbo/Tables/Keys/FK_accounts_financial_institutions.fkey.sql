ALTER TABLE [dbo].[accounts]
    ADD CONSTRAINT [FK_accounts_financial_institutions] FOREIGN KEY ([id_financial_institution]) REFERENCES [dbo].[financial_institutions] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


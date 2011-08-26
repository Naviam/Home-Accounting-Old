ALTER TABLE [dbo].[financial_institution_info]
    ADD CONSTRAINT [FK_financial_institution_info_financial_institutions] FOREIGN KEY ([id_fin]) REFERENCES [dbo].[financial_institutions] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


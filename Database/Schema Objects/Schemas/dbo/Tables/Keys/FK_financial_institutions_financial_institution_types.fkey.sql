ALTER TABLE [dbo].[financial_institutions]
    ADD CONSTRAINT [FK_financial_institutions_financial_institution_types] FOREIGN KEY ([id_type]) REFERENCES [dbo].[financial_institution_types] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


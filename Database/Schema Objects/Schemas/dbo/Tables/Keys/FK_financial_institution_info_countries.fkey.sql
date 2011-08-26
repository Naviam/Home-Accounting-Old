ALTER TABLE [dbo].[financial_institution_info]
    ADD CONSTRAINT [FK_financial_institution_info_countries] FOREIGN KEY ([id_country]) REFERENCES [dbo].[countries] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


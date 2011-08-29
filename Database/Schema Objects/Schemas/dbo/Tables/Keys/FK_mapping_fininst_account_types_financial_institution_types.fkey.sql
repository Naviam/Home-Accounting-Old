ALTER TABLE [dbo].[mapping_fininst_account_types]
    ADD CONSTRAINT [FK_mapping_fininst_account_types_financial_institution_types] FOREIGN KEY ([id_fin_type]) REFERENCES [dbo].[financial_institution_types] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


ALTER TABLE [dbo].[mapping_fininst_account_types]
    ADD CONSTRAINT [FK_mapping_fininst_account_types_account_types] FOREIGN KEY ([id_acc_type]) REFERENCES [dbo].[account_types] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


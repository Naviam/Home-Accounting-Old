ALTER TABLE [dbo].[mapping_fininst_account_types]
    ADD CONSTRAINT [PK_mapping_fininst_account_types] PRIMARY KEY CLUSTERED ([id_fin_type] ASC, [id_acc_type] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);


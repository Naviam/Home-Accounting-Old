ALTER TABLE [dbo].[transactions]
    ADD CONSTRAINT [FK_transactions_categories] FOREIGN KEY ([id_category]) REFERENCES [dbo].[categories] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


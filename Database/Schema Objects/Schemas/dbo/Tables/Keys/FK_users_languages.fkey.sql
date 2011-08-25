ALTER TABLE [dbo].[users]
    ADD CONSTRAINT [FK_users_languages] FOREIGN KEY ([id_language]) REFERENCES [dbo].[languages] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


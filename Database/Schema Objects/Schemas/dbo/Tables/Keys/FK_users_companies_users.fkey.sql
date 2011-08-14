ALTER TABLE [dbo].[users_companies]
    ADD CONSTRAINT [FK_users_companies_users] FOREIGN KEY ([id_user]) REFERENCES [dbo].[users] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


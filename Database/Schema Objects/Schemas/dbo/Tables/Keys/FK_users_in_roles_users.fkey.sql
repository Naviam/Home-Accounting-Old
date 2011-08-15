ALTER TABLE [dbo].[users_in_roles]
    ADD CONSTRAINT [FK_users_in_roles_users] FOREIGN KEY ([id_user]) REFERENCES [dbo].[users] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


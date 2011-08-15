ALTER TABLE [dbo].[users_in_roles]
    ADD CONSTRAINT [FK_users_in_roles_roles] FOREIGN KEY ([id_role]) REFERENCES [dbo].[roles] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


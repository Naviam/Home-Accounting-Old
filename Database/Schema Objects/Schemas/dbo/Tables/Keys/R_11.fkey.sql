ALTER TABLE [dbo].[users_currencies]
    ADD CONSTRAINT [R_11] FOREIGN KEY ([id_user]) REFERENCES [dbo].[users] ([id]) ON DELETE CASCADE ON UPDATE NO ACTION;


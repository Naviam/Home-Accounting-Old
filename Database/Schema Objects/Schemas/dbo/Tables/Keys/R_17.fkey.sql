ALTER TABLE [dbo].[companies]
    ADD CONSTRAINT [R_17] FOREIGN KEY ([id_user]) REFERENCES [dbo].[users] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


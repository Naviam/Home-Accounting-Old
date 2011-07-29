ALTER TABLE [dbo].[categories]
    ADD CONSTRAINT [R_28] FOREIGN KEY ([id_user]) REFERENCES [dbo].[users] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


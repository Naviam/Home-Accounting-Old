ALTER TABLE [dbo].[users_currencies]
    ADD CONSTRAINT [R_12] FOREIGN KEY ([id_curency_from]) REFERENCES [dbo].[currencies] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


ALTER TABLE [dbo].[accounts]
    ADD CONSTRAINT [R_19] FOREIGN KEY ([id_company]) REFERENCES [dbo].[companies] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


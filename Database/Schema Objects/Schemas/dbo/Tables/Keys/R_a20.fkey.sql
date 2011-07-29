ALTER TABLE [dbo].[company_personnel]
    ADD CONSTRAINT [R_a20] FOREIGN KEY ([id_company]) REFERENCES [dbo].[companies] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


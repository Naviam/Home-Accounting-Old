ALTER TABLE [dbo].[users_companies]
    ADD CONSTRAINT [FK_users_companies_companies] FOREIGN KEY ([id_company]) REFERENCES [dbo].[companies] ([id]) ON DELETE CASCADE ON UPDATE NO ACTION;


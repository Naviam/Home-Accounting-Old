ALTER TABLE [dbo].[users_currencies]
    ADD CONSTRAINT [R_13] FOREIGN KEY ([id_currency_to]) REFERENCES [dbo].[currencies] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


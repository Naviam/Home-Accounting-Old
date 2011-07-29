ALTER TABLE [dbo].[exchange_rates]
    ADD CONSTRAINT [R_9] FOREIGN KEY ([id_country]) REFERENCES [dbo].[countries] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;


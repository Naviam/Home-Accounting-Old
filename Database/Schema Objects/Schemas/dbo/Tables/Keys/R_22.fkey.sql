﻿ALTER TABLE [dbo].[tags]
    ADD CONSTRAINT [R_22] FOREIGN KEY ([id_user]) REFERENCES [dbo].[users] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;

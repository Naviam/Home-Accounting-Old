﻿ALTER TABLE [dbo].[users_in_roles]
    ADD CONSTRAINT [PK_users_in_roles] PRIMARY KEY CLUSTERED ([id_user] ASC, [id_role] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);


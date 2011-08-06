-- =============================================
-- Script Template
-- =============================================
USE [naviam]
GO

SET IDENTITY_INSERT [dbo].[users] ON
INSERT [dbo].[users] ([id], [email], [password]) VALUES (1, N's@s.s', N'1')
SET IDENTITY_INSERT [dbo].[users] OFF
GO

SET IDENTITY_INSERT [dbo].[countries] ON
INSERT [dbo].[countries] ([id], [name], [short_name]) VALUES (1, N'Belarus', N'BLR')
SET IDENTITY_INSERT [dbo].[countries] OFF
GO

SET IDENTITY_INSERT [dbo].[company_types] ON
INSERT [dbo].[company_types] ([id], [name]) VALUES (1, N'home')
SET IDENTITY_INSERT [dbo].[company_types] OFF
GO

SET IDENTITY_INSERT [dbo].[companies] ON
INSERT [dbo].[companies] ([id], [id_country], [id_user], [id_company_type], [name_business]) 
				  VALUES (1, 1, 1, 1, N'Home SS')
SET IDENTITY_INSERT [dbo].[companies] OFF
GO

SET IDENTITY_INSERT [dbo].[currencies] ON
INSERT [dbo].[currencies] (id, name_short) VALUES (1, N'BRB')
SET IDENTITY_INSERT [dbo].[currencies] OFF
GO

SET IDENTITY_INSERT [dbo].[accounts] ON
INSERT [dbo].[accounts] ([id], [date_creation], [number], [id_company], [type], [id_currency]) 
				 VALUES (1,  GETUTCDATE(), 1, 1, N'home', 1)
SET IDENTITY_INSERT [dbo].[accounts] OFF
GO

--SET IDENTITY_INSERT [dbo].[categories] ON
--INSERT [dbo].[categories] ([id], [name], [parent_id], [id_user]) VALUES (1, N'Auto&Transport', NULL, NULL)
--SET IDENTITY_INSERT [dbo].[categories] OFF

SET IDENTITY_INSERT [dbo].[transactions] ON
INSERT [dbo].[transactions] ([id], [date], [amount], [merchant], [id_account], [id_category], [description], [notes], [type], [direction]) 
					 VALUES (1,  GETUTCDATE(), 100, N'ATM #1', 1, 1, N'test description', N'test notes', N'cash', N'income')
INSERT [dbo].[transactions] ([id], [date], [amount], [merchant], [id_account], [id_category], [description], [notes], [type], [direction]) 
					 VALUES (2,  GETUTCDATE(), 50, N'ProStore', 1, 1, N'test ProStore', N'ProStore notes', N'cash', N'expense')					 
SET IDENTITY_INSERT [dbo].[transactions] OFF
GO
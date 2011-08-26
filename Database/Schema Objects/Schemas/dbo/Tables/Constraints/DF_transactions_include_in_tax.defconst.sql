ALTER TABLE [dbo].[transactions]
    ADD CONSTRAINT [DF_transactions_include_in_tax] DEFAULT ((0)) FOR [include_in_tax];


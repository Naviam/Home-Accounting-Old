CREATE TABLE [dbo].[sms_accounts] (
    [id_account] INT           NOT NULL,
    [device]     NVARCHAR (50) NOT NULL,
    [date_start] SMALLDATETIME NULL,
    [date_end]   SMALLDATETIME NULL
);


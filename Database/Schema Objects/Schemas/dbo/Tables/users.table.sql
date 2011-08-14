CREATE TABLE [dbo].[users] (
    [id]                                          INT            IDENTITY (1, 1) NOT NULL,
    [email]                                       NVARCHAR (256) COLLATE Latin1_General_CS_AS NOT NULL,
    [password]                                    NVARCHAR (200) COLLATE Latin1_General_CS_AS NULL,
    [first_name]                                  NVARCHAR (50)  NULL,
    [last_name]                                   NVARCHAR (50)  NULL,
    [comment]                                     NVARCHAR (256) NULL,
    [password_question]                           NVARCHAR (256) NULL,
    [password_answer]                             NVARCHAR (256) NULL,
    [is_approved]                                 BIT            NULL,
    [last_activity_date]                          DATETIME       NULL,
    [last_login_date]                             DATETIME       NULL,
    [last_password_changed_date]                  DATETIME       NULL,
    [creation_date]                               DATETIME       NULL,
    [is_locked_out]                               BIT            NULL,
    [last_locked_out_date]                        DATETIME       NULL,
    [failed_password_attempt_count]               INT            NULL,
    [failed_password_attempt_window_start]        DATETIME       NULL,
    [failed_password_answer_attempt_count]        INT            NULL,
    [failed_password_answer_attempt_window_start] DATETIME       NULL
);


-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Get user
-- =============================================
CREATE PROCEDURE [web].[user_get]
	@email nvarchar(50)
AS
BEGIN
  SELECT u.id, u.email, u.[password], u.first_name, u.last_name, u.comment, u.password_question, u.password_answer,
		 u.is_approved, u.last_activity_date, u.last_login_date, u.last_password_changed_date, u.creation_date,
		 u.is_locked_out, u.last_locked_out_date, u.failed_password_attempt_count, 
		 u.failed_password_attempt_window_start, u.failed_password_answer_attempt_count, 
		 u.failed_password_answer_attempt_window_start, u.id_language, l.name_short as language_name_short 
	FROM users u
		 LEFT JOIN languages l ON l.id= u.id_language 
			WHERE LOWER(u.email) = LOWER(@email)
END
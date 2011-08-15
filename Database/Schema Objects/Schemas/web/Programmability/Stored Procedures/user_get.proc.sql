-- =============================================
-- Author:		Pavel Mironchik
-- Create date: 29/07/2011
-- Description:	Get user
-- =============================================
CREATE PROCEDURE [web].[user_get]
	@email nvarchar(50)
AS
BEGIN
  SELECT id, email, [password], first_name, last_name, comment, password_question, password_answer,
		 is_approved, last_activity_date, last_login_date, last_password_changed_date, creation_date,
		 is_locked_out, last_locked_out_date, failed_password_attempt_count, 
		 failed_password_attempt_window_start, failed_password_answer_attempt_count, 
		 failed_password_answer_attempt_window_start 
	FROM users 
		WHERE users.email = @email
END
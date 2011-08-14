CREATE PROCEDURE [web].[user_change_password_question_and_answer]
    @email              nvarchar(256),
    @new_password_question   nvarchar(256),
    @new_password_answer     nvarchar(256)
AS
BEGIN
    DECLARE @UserId int
    SELECT  @UserId = NULL
    SELECT  @UserId = id
		FROM    users
			WHERE   LOWER(@email)= LOWER(email)
    IF (@UserId IS NULL)
    BEGIN
        RETURN(1)
    END
    UPDATE dbo.users
		SET    password_question = @new_password_question, password_answer = @new_password_answer
			WHERE  id=@UserId
    RETURN(0)
END
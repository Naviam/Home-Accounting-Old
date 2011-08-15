CREATE PROCEDURE [web].[user_create]
    @email									nvarchar(256),
    @password                               nvarchar(200),
    @password_question                      nvarchar(256),
    @password_answer	                    nvarchar(128),
    @first_name								nvarchar(50) = null,
    @last_name								nvarchar(50) = null,
    @comment								nvarchar(256) = null,
    @is_approved                            bit,
    @current_time_utc                       datetime,
    @unique_email                           int      = 1,
    @id										int OUTPUT
AS
BEGIN
    DECLARE @IsLockedOut bit
    SET @IsLockedOut = 0

    DECLARE @LastLockoutDate  datetime
    SET @LastLockoutDate = CONVERT( datetime, '17540101', 112 )

    DECLARE @FailedPasswordAttemptCount int
    SET @FailedPasswordAttemptCount = 0

    DECLARE @FailedPasswordAttemptWindowStart  datetime
    SET @FailedPasswordAttemptWindowStart = CONVERT( datetime, '17540101', 112 )

    DECLARE @FailedPasswordAnswerAttemptCount int
    SET @FailedPasswordAnswerAttemptCount = 0

    DECLARE @FailedPasswordAnswerAttemptWindowStart  datetime
    SET @FailedPasswordAnswerAttemptWindowStart = CONVERT( datetime, '17540101', 112 )

    DECLARE @NewUserCreated bit
    DECLARE @ReturnValue   int
    SET @ReturnValue = 0

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
    	SET @TranStarted = 0

    IF (@unique_email = 1)
    BEGIN
        IF (EXISTS (SELECT *
                    FROM  dbo.users WITH ( UPDLOCK, HOLDLOCK )
                    WHERE LOWER(email) = LOWER(@Email)))
        BEGIN
            SET @ErrorCode = 7
            GOTO Cleanup
        END
    END

    INSERT INTO dbo.users
                (email, [password], first_name, last_name, comment, password_question, 
                 password_answer, is_approved, last_activity_date, last_login_date, last_password_changed_date,
                 creation_date, is_locked_out, last_locked_out_date, failed_password_attempt_count,
                 failed_password_attempt_window_start, failed_password_answer_attempt_count, failed_password_answer_attempt_window_start)
         VALUES (@email,@password,@first_name,@last_name,@comment,@password_question,
				 @password_answer,@is_approved,@current_time_utc,@current_time_utc,@current_time_utc,
				 @current_time_utc, @IsLockedOut, @LastLockoutDate, @FailedPasswordAttemptCount,
				 @FailedPasswordAttemptWindowStart, @FailedPasswordAnswerAttemptCount, @FailedPasswordAnswerAttemptWindowStart)

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF( @TranStarted = 1 )
    BEGIN
	    SET @TranStarted = 0
	    COMMIT TRANSACTION
    END

    RETURN 0

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
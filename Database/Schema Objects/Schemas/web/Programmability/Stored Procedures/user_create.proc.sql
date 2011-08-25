CREATE PROCEDURE [web].[user_create]
    @email									nvarchar(256),
    @password                               nvarchar(200),
    @password_question                      nvarchar(256) = null,
    @password_answer	                    nvarchar(128)= null,
    @first_name								nvarchar(50) = null,
    @last_name								nvarchar(50) = null,
    @comment								nvarchar(256) = null,
    @is_approved                            bit = null,
    @current_time_utc                       datetime = null,
    @id										int OUTPUT
AS
BEGIN
    
    if @current_time_utc is null SET @current_time_utc = GETUTCDATE();
    
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

    INSERT INTO dbo.users
                (email, [password], first_name, last_name, comment, password_question, 
                 password_answer, is_approved, last_activity_date, last_login_date, last_password_changed_date,
                 creation_date, is_locked_out, last_locked_out_date, failed_password_attempt_count,
                 failed_password_attempt_window_start, failed_password_answer_attempt_count, failed_password_answer_attempt_window_start)
         VALUES (@email,@password,@first_name,@last_name,@comment,@password_question,
				 @password_answer,@is_approved,@current_time_utc,@current_time_utc,@current_time_utc,
				 @current_time_utc, @IsLockedOut, @LastLockoutDate, @FailedPasswordAttemptCount,
				 @FailedPasswordAttemptWindowStart, @FailedPasswordAnswerAttemptCount, @FailedPasswordAnswerAttemptWindowStart)
	
	SET @id = SCOPE_IDENTITY();
	
END
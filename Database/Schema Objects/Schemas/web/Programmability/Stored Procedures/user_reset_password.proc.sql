CREATE PROCEDURE [web].[user_reset_password]
    @email							 nvarchar(256),
    @new_password					 nvarchar(200),
    @max_invalid_password_attempts	 int,
    @password_attempt_window		 int,
    @current_time_utc				 datetime,
    @password_answer				 nvarchar(256) = NULL
AS
BEGIN
    DECLARE @IsLockedOut                            bit
    DECLARE @LastLockoutDate                        datetime
    DECLARE @FailedPasswordAttemptCount             int
    DECLARE @FailedPasswordAttemptWindowStart       datetime
    DECLARE @FailedPasswordAnswerAttemptCount       int
    DECLARE @FailedPasswordAnswerAttemptWindowStart datetime

    DECLARE @UserId                                 int
    SET     @UserId = NULL

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

    SELECT  @UserId = u.id
		FROM    dbo.users u
			WHERE   LOWER(u.email) = LOWER(@email)
    
    IF ( @UserId IS NULL )
    BEGIN
        SET @ErrorCode = 1
        GOTO Cleanup
    END

    SELECT @IsLockedOut = is_locked_out,
           @LastLockoutDate = last_locked_out_date,
           @FailedPasswordAttemptCount = failed_password_attempt_count,
           @FailedPasswordAttemptWindowStart = failed_password_attempt_window_start,
           @FailedPasswordAnswerAttemptCount = failed_password_answer_attempt_count,
           @FailedPasswordAnswerAttemptWindowStart = failed_password_answer_attempt_window_start
    FROM dbo.users WITH ( UPDLOCK )
    WHERE @UserId = id

    IF( @IsLockedOut = 1 )
    BEGIN
        SET @ErrorCode = 99
        GOTO Cleanup
    END

    UPDATE dbo.users
    SET    [password] = @new_password,
           last_password_changed_date = @current_time_utc
		WHERE  @UserId = id AND
			( ( @password_answer IS NULL ) OR ( LOWER( password_answer ) = LOWER( @password_answer ) ) )

    IF ( @@ROWCOUNT = 0 )
        BEGIN
            IF( @current_time_utc > DATEADD( minute, @password_attempt_window, @FailedPasswordAnswerAttemptWindowStart ) )
            BEGIN
                SET @FailedPasswordAnswerAttemptWindowStart = @current_time_utc
                SET @FailedPasswordAnswerAttemptCount = 1
            END
            ELSE
            BEGIN
                SET @FailedPasswordAnswerAttemptWindowStart = @current_time_utc
                SET @FailedPasswordAnswerAttemptCount = @FailedPasswordAnswerAttemptCount + 1
            END

            BEGIN
                IF( @FailedPasswordAnswerAttemptCount >= @max_invalid_password_attempts )
                BEGIN
                    SET @IsLockedOut = 1
                    SET @LastLockoutDate = @current_time_utc
                END
            END

            SET @ErrorCode = 3
        END
    ELSE
        BEGIN
            IF( @FailedPasswordAnswerAttemptCount > 0 )
            BEGIN
                SET @FailedPasswordAnswerAttemptCount = 0
                SET @FailedPasswordAnswerAttemptWindowStart = CONVERT( datetime, '17540101', 112 )
            END
        END

    IF( NOT ( @password_answer IS NULL ) )
    BEGIN
        UPDATE dbo.users
        SET is_locked_out = @IsLockedOut, last_locked_out_date = @LastLockoutDate,
            failed_password_attempt_count = @FailedPasswordAttemptCount,
            failed_password_attempt_window_start = @FailedPasswordAttemptWindowStart,
            failed_password_answer_attempt_count = @FailedPasswordAnswerAttemptCount,
            failed_password_answer_attempt_window_start = @FailedPasswordAnswerAttemptWindowStart
        WHERE @UserId = id

        IF( @@ERROR <> 0 )
        BEGIN
            SET @ErrorCode = -1
            GOTO Cleanup
        END
    END

    IF( @TranStarted = 1 )
    BEGIN
	SET @TranStarted = 0
	COMMIT TRANSACTION
    END

    RETURN @ErrorCode

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
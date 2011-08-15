CREATE PROCEDURE [web].[user_update]
    @email		          nvarchar(256),
    @comment              ntext,
    @is_approved          bit,
    @last_login_date      datetime,
    @last_activity_date   datetime,
    @current_time_utc     datetime
AS
BEGIN
    DECLARE @UserId int
    SELECT  @UserId = NULL
    
    SELECT @UserId = u.id
		FROM dbo.users u
			WHERE u.email = @email

    IF (@UserId IS NULL)
        RETURN(1)

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
	SET @TranStarted = 0

    UPDATE dbo.users WITH (ROWLOCK)
    SET
         last_activity_date   = @last_activity_date,
         email			      = @email,
         comment			  = @comment,
         is_approved		  = @is_approved,
         last_login_date	  = @last_login_date
    WHERE
       @UserId = id

    IF( @@ERROR <> 0 )
        GOTO Cleanup
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

    RETURN -1
END
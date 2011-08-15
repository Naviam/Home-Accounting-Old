CREATE PROCEDURE [web].[user_delete]
    @email         nvarchar(256)AS
BEGIN
    DECLARE @UserId               int
    SELECT  @UserId               = NULL
    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
	SET @TranStarted = 0

    DECLARE @ErrorCode   int
    DECLARE @RowCount    int

    SET @ErrorCode = 0
    SET @RowCount  = 0

    SELECT  @UserId = u.id
		FROM dbo.users u
			WHERE LOWER(u.email) = LOWER(@email)

    IF (@UserId IS NULL)
    BEGIN
        GOTO Cleanup
    END

    -- Delete from transactions 
    DELETE FROM dbo.transactions
		WHERE id IN (
		 SELECT tr.id 
			FROM transactions tr
				INNER JOIN accounts ac ON tr.id_account = ac.id
				INNER JOIN companies c ON ac.id_company = c.id and
					              c.id IN (SELECT uc.id_company FROM users_companies uc WHERE uc.id_user = @UserId)  
					              )
        SELECT @ErrorCode = @@ERROR,
               @RowCount = @@ROWCOUNT

        IF( @ErrorCode <> 0 )
            GOTO Cleanup

    -- Delete from accounts
    DELETE FROM dbo.accounts
		WHERE id IN (
		 SELECT ac.id 
			FROM accounts ac
				 INNER JOIN companies c ON ac.id_company = c.id and
					              c.id IN (SELECT uc.id_company FROM users_companies uc WHERE uc.id_user = @UserId)  
					              )
        SELECT @ErrorCode = @@ERROR,
               @RowCount = @@ROWCOUNT

        IF( @ErrorCode <> 0 )
            GOTO Cleanup
            
    -- Delete from company_personnel
    DELETE FROM dbo.company_personnel
		WHERE id_company IN (SELECT uc.id_company FROM users_companies uc WHERE uc.id_user = @UserId)
        
        SELECT @ErrorCode = @@ERROR,
               @RowCount = @@ROWCOUNT

        IF( @ErrorCode <> 0 )
            GOTO Cleanup
            
    -- Delete from company
    DELETE FROM dbo.companies
		WHERE id IN (SELECT uc.id_company FROM users_companies uc WHERE uc.id_user = @UserId)
        
        SELECT @ErrorCode = @@ERROR,
               @RowCount = @@ROWCOUNT

        IF( @ErrorCode <> 0 )
            GOTO Cleanup
            
    -- Delete from users_in_roles
    DELETE FROM dbo.users_in_roles
		WHERE id_user = @UserId
        
        SELECT @ErrorCode = @@ERROR,
               @RowCount = @@ROWCOUNT

        IF( @ErrorCode <> 0 )
            GOTO Cleanup

    -- Delete from users
    DELETE FROM dbo.users
		WHERE id = @UserId
        
        SELECT @ErrorCode = @@ERROR,
               @RowCount = @@ROWCOUNT

        IF( @ErrorCode <> 0 )
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

    RETURN @ErrorCode

END
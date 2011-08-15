
CREATE PROCEDURE [web].[role_del]
    @role_name						nvarchar(256),
    @delete_only_if_role_is_empty   bit
AS
BEGIN
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

    DECLARE @RoleId   int
    SELECT  @RoleId = NULL
    SELECT  @RoleId = id FROM dbo.roles WHERE LOWER(name) = LOWER(@role_name)

    IF (@RoleId IS NULL)
    BEGIN
        SELECT @ErrorCode = 1
        GOTO Cleanup
    END
    IF (@delete_only_if_role_is_empty <> 0)
    BEGIN
        IF (EXISTS (SELECT id_role FROM dbo.users_in_roles  WHERE @RoleId = id_role))
        BEGIN
            SELECT @ErrorCode = 2
            GOTO Cleanup
        END
    END


    DELETE FROM dbo.users_in_roles  WHERE @RoleId = id_role

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DELETE FROM dbo.roles WHERE @RoleId = id

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

    RETURN(0)

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode
END
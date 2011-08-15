
CREATE PROCEDURE [web].[users_in_roles_remove_users_from_roles]
	@emails		  nvarchar(4000),
	@roles		  nvarchar(4000)
AS
BEGIN
	DECLARE @TranStarted   bit
	SET @TranStarted = 0

	IF( @@TRANCOUNT = 0 )
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @tbNames  table(Name nvarchar(256) COLLATE Latin1_General_CS_AS NOT NULL PRIMARY KEY)
	DECLARE @tbRoles  table(RoleId int NOT NULL PRIMARY KEY)
	DECLARE @tbUsers  table(UserId int NOT NULL PRIMARY KEY)
	DECLARE @Num	  int
	DECLARE @Pos	  int
	DECLARE @NextPos  int
	DECLARE @Name	  nvarchar(256)
	DECLARE @CountAll int
	DECLARE @CountU	  int
	DECLARE @CountR	  int


	SET @Num = 0
	SET @Pos = 1
	WHILE(@Pos <= LEN(@roles))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @roles,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@roles) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@roles, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos+1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbRoles
	  SELECT id
		FROM   dbo.roles ar, @tbNames t
			WHERE  LOWER(t.Name COLLATE Latin1_General_CS_AS) = LOWER(ar.name)
	SELECT @CountR = @@ROWCOUNT

	IF (@CountR <> @Num)
	BEGIN
		SELECT TOP 1 N'', Name
			FROM   @tbNames
				WHERE  LOWER(Name COLLATE Latin1_General_CS_AS) NOT IN (SELECT LOWER(ar.name) FROM dbo.roles ar, @tbRoles r WHERE r.RoleId = ar.id)
		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(2)
	END


	DELETE FROM @tbNames WHERE 1=1
	SET @Num = 0
	SET @Pos = 1


	WHILE(@Pos <= LEN(@emails))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @emails,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@emails) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@emails, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos+1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbUsers
	  SELECT id
		FROM   dbo.users ar, @tbNames t
			WHERE  LOWER(t.Name) = LOWER(ar.email)

	SELECT @CountU = @@ROWCOUNT
	IF (@CountU <> @Num)
	BEGIN
		SELECT TOP 1 Name, N''
			FROM   @tbNames
				WHERE  LOWER(Name) NOT IN (SELECT LOWER(au.email) FROM dbo.users au,  @tbUsers u WHERE u.UserId = au.id)

		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(1)
	END

	SELECT  @CountAll = COUNT(*)
		FROM dbo.users_in_roles ur, @tbUsers u, @tbRoles r
			WHERE   ur.id_user = u.UserId AND ur.id_role = r.RoleId

	IF (@CountAll <> @CountU * @CountR)
	BEGIN
		SELECT TOP 1 u.email, r.name
			FROM		 @tbUsers tu, @tbRoles tr, dbo.users u, dbo.roles r
				WHERE		 u.id = tu.UserId AND r.id = tr.RoleId AND
							 tu.UserId NOT IN (SELECT ur.id_user FROM dbo.users_in_roles ur WHERE ur.id_role = tr.RoleId) AND
							 tr.RoleId NOT IN (SELECT ur.id_role FROM dbo.users_in_roles ur WHERE ur.id_user = tu.UserId)
		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(3)
	END

	DELETE FROM dbo.users_in_roles
	WHERE id_user IN (SELECT UserId FROM @tbUsers)
	  AND id_role IN (SELECT RoleId FROM @tbRoles)
	IF( @TranStarted = 1 )
		COMMIT TRANSACTION
	RETURN(0)
END
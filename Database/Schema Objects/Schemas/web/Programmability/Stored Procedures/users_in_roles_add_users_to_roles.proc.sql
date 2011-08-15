
CREATE PROCEDURE [web].[users_in_roles_add_users_to_roles]
	@emails			  nvarchar(4000),
	@roles			  nvarchar(4000),
	@current_time_utc datetime
AS
BEGIN
	DECLARE @TranStarted   bit
	SET @TranStarted = 0

	IF( @@TRANCOUNT = 0 )
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @tbNames	table(Name nvarchar(256) COLLATE Latin1_General_CS_AS NOT NULL PRIMARY KEY)
	DECLARE @tbRoles	table(RoleId int NOT NULL PRIMARY KEY)
	DECLARE @tbUsers	table(UserId int NOT NULL PRIMARY KEY)
	DECLARE @Num		int
	DECLARE @Pos		int
	DECLARE @NextPos	int
	DECLARE @Name		nvarchar(256)

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

	IF (@@ROWCOUNT <> @Num)
	BEGIN
		SELECT TOP 1 Name
			FROM   @tbNames
				WHERE  LOWER(Name COLLATE Latin1_General_CS_AS) NOT IN (SELECT LOWER(ar.name) FROM dbo.roles ar,  @tbRoles r WHERE r.RoleId = ar.id)
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

	IF (@@ROWCOUNT <> @Num)
	BEGIN
		DELETE FROM @tbNames
			WHERE LOWER(Name) IN (SELECT LOWER(au.email) FROM users au,  @tbUsers u WHERE au.id = u.UserId)

		INSERT dbo.users(email,last_activity_date)
		  SELECT Name,@current_time_utc
			FROM   @tbNames

		INSERT INTO @tbUsers
		  SELECT  id
			FROM	dbo.users au, @tbNames t
		  WHERE   LOWER(t.Name) = LOWER(au.email)
	END

	IF (EXISTS (SELECT * FROM dbo.users_in_roles ur, @tbUsers tu, @tbRoles tr WHERE tu.UserId = ur.id_user AND tr.RoleId = ur.id_role))
	BEGIN
		SELECT TOP 1 u.email, r.name
			FROM dbo.users_in_roles ur, @tbUsers tu, @tbRoles tr, users u, roles r
				WHERE u.id = tu.UserId AND r.id = tr.RoleId AND tu.UserId = ur.id_user AND tr.RoleId = ur.id_role

		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(3)
	END

	INSERT INTO dbo.users_in_roles (id_user, id_role)
		SELECT UserId, RoleId
			FROM @tbUsers, @tbRoles

	IF( @TranStarted = 1 )
		COMMIT TRANSACTION
	RETURN(0)
END
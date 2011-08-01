ALTER DATABASE [$(DatabaseName)]
    ADD LOG FILE (NAME = [naviam_log], FILENAME = 'c:\Program Files\Microsoft SQL Server\MSSQL10_50.SQLEXPRESS\MSSQL\DATA\naviam_log.ldf', SIZE = 1024 KB, MAXSIZE = 2097152 MB, FILEGROWTH = 10 %);


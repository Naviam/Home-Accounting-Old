﻿ALTER DATABASE [$(DatabaseName)]
    ADD FILE (NAME = [naviam], FILENAME = 'c:\Program Files\Microsoft SQL Server\MSSQL10_50.SQLEXPRESS\MSSQL\DATA\naviam.mdf', SIZE = 3072 KB, FILEGROWTH = 1024 KB) TO FILEGROUP [PRIMARY];


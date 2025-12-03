-- This script runs automatically when the MSSQL container starts for the first time
-- because it is mounted to /docker-entrypoint-initdb.d/ in the docker-compose.yml.

-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '__DB_NAME__')
BEGIN
    CREATE DATABASE __DB_NAME__;
END
GO

USE __DB_NAME__;
GO

-- Define the login name and password for the application

-- Create a SQL Server Login (Server-level identity)
IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = '__DB_USER__')
BEGIN
    CREATE LOGIN [__DB_USER__] 
    WITH PASSWORD = '__DB_PASSWORD__', 
    CHECK_POLICY = ON, 
    CHECK_EXPIRATION = OFF;
END
GO

-- Create a User within the new database and map it to the Login
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '__DB_USER__')
BEGIN
    CREATE USER [__DB_USER__] FOR LOGIN [__DB_USER__];
END
GO

-- Grant necessary permissions (The minimum required for EF Core to operate)
EXEC sp_addrolemember 'db_datareader', '__DB_USER__'; -- Allows SELECT (read data)
EXEC sp_addrolemember 'db_datawriter', '__DB_USER__'; -- Allows INSERT, UPDATE, DELETE (write/modify data)
EXEC sp_addrolemember 'db_ddladmin', '__DB_USER__'; -- Allows schema changes (CREATE, ALTER, DROP) required by EF Core Migrations
GO
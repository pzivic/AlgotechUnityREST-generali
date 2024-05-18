-- Predrag Živić
-- 06.03.2024
-- Dodavanje logina

USE OnBaseUTL
GO

CREATE USER [onbase_utl_login] FOR LOGIN [onbase_utl_login] WITH DEFAULT_SCHEMA=[dbo]


-- Provera da li login postoji
IF EXISTS (SELECT name FROM sys.server_principals WHERE name = 'onbase_utl_login') 
BEGIN
	EXEC sp_addrolemember 'db_datareader', 'onbase_utl_login'
	--EXEC sp_addrolemember 'db_datawriter', 'onbase_utl_login'
END
ELSE
BEGIN
    PRINT 'Greška';
END





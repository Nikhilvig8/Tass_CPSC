-- ============================================================================
-- CPSC(Tata) - production DB script to enable app-level TOTP MFA
-- (Users.cs GetTotpSecret / SetTotpSecret; App_Code/TotpHelper.cs)
--
-- IMPORTANT - UNVERIFIED AGAINST THIS DATABASE: this script was ported from a sibling
-- project whose author verified the [LOGIN]/tblEmployees schema against a restored
-- copy of a database named "Tata". THIS project's connection string
-- (CPSC(Tata)/Web.config, "constr") points at a DIFFERENT database:
--   Data Source=172.31.63.17; Initial Catalog=New_TassDB
-- No assumption is made here that New_TassDB has the same table/column names.
-- Before running this: connect to New_TassDB yourself and confirm
--   1) the table (and column) that dbo.ValidateUser / vw_Users actually reads
--      the login/password from - it may not be named dbo.tblEmployees, and the
--      login column may not be named [LOGIN];
--   2) that table has no existing column already serving this purpose.
-- Then edit the three object names below (search for TABLE_NAME_HERE and
-- LOGIN_COLUMN_HERE) to match before executing.
--
-- Idempotent as written: safe to run again if partially applied already.
-- ============================================================================

USE [New_TassDB];
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.TABLE_NAME_HERE') AND name = 'TotpSecret'
)
BEGIN
    ALTER TABLE dbo.TABLE_NAME_HERE ADD TotpSecret varchar(64) NULL;
END
GO

-- CREATE OR ALTER needs compat level 130+ (SQL Server 2016+); using the
-- drop-then-create form instead so this also runs on older production instances.
IF OBJECT_ID('dbo.GetTotpSecret', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetTotpSecret;
GO

CREATE PROCEDURE dbo.GetTotpSecret
    @Username varchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TotpSecret
    FROM dbo.TABLE_NAME_HERE
    WHERE [LOGIN_COLUMN_HERE] = @Username;
END
GO

IF OBJECT_ID('dbo.SetTotpSecret', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SetTotpSecret;
GO

CREATE PROCEDURE dbo.SetTotpSecret
    @Username varchar(50),
    @TotpSecret varchar(64)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.TABLE_NAME_HERE
    SET TotpSecret = @TotpSecret
    WHERE [LOGIN_COLUMN_HERE] = @Username;
END
GO

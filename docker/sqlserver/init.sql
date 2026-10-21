-- Run by the sqlserver-init compose service. Safe to re-run.
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'InvoiceTrackerUser')
    CREATE LOGIN InvoiceTrackerUser WITH PASSWORD = '$(APP_DB_PASSWORD)', CHECK_POLICY = ON;
ELSE
    ALTER LOGIN InvoiceTrackerUser WITH PASSWORD = '$(APP_DB_PASSWORD)';
GO
-- Lets EF Core migrations create each service database; the creator becomes its dbo.
IF IS_SRVROLEMEMBER('dbcreator', 'InvoiceTrackerUser') = 0
    ALTER SERVER ROLE dbcreator ADD MEMBER InvoiceTrackerUser;
GO
PRINT 'InvoiceTrackerUser ready (dbcreator).';
GO

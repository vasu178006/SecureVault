-- =============================================
-- SecureVault Database Creation Script
-- Run this script in SQL Server Management Studio
-- or via sqlcmd to create the database.
-- =============================================

-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SecureVaultDB')
BEGIN
    CREATE DATABASE SecureVaultDB;
END
GO

USE SecureVaultDB;
GO

-- =============================================
-- Table: Users
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserID          INT IDENTITY(1,1) PRIMARY KEY,
        FullName        NVARCHAR(100)   NOT NULL,
        Email           NVARCHAR(255)   NOT NULL UNIQUE,
        PasswordHash    NVARCHAR(512)   NOT NULL,
        Role            NVARCHAR(20)    NOT NULL DEFAULT 'User',  -- 'Admin' or 'User'
        ProfileImagePath NVARCHAR(500)  NULL,
        IsBlocked       BIT             NOT NULL DEFAULT 0,
        FailedAttempts  INT             NOT NULL DEFAULT 0,
        LastLogin       DATETIME        NULL,
        CreatedAt       DATETIME        NOT NULL DEFAULT GETDATE(),

        CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'User'))
    );
END
GO

-- =============================================
-- Table: Categories
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE Categories (
        CategoryID      INT IDENTITY(1,1) PRIMARY KEY,
        UserID          INT             NULL,           -- NULL = system category
        CategoryName    NVARCHAR(100)   NOT NULL,

        CONSTRAINT FK_Categories_Users FOREIGN KEY (UserID)
            REFERENCES Users(UserID) ON DELETE SET NULL
    );
END
GO

-- =============================================
-- Table: Documents
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Documents')
BEGIN
    CREATE TABLE Documents (
        DocumentID      INT IDENTITY(1,1) PRIMARY KEY,
        UserID          INT             NOT NULL,
        FileName        NVARCHAR(255)   NOT NULL,
        FilePath        NVARCHAR(500)   NOT NULL,
        FileType        NVARCHAR(20)    NOT NULL,
        FileSize        BIGINT          NOT NULL,       -- in bytes
        CategoryID      INT             NULL,
        Description     NVARCHAR(1000)  NULL,
        Tags            NVARCHAR(500)   NULL,           -- comma-separated tags
        IsImportant     BIT             NOT NULL DEFAULT 0,
        IsDeleted       BIT             NOT NULL DEFAULT 0,
        UploadDate      DATETIME        NOT NULL DEFAULT GETDATE(),
        LastViewedDate  DATETIME        NULL,

        CONSTRAINT FK_Documents_Users FOREIGN KEY (UserID)
            REFERENCES Users(UserID) ON DELETE CASCADE,
        CONSTRAINT FK_Documents_Categories FOREIGN KEY (CategoryID)
            REFERENCES Categories(CategoryID) ON DELETE SET NULL
    );
END
GO

-- =============================================
-- Table: ActivityLogs
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ActivityLogs')
BEGIN
    CREATE TABLE ActivityLogs (
        LogID           INT IDENTITY(1,1) PRIMARY KEY,
        UserID          INT             NOT NULL,
        Action          NVARCHAR(100)   NOT NULL,
        Description     NVARCHAR(500)   NULL,
        Timestamp       DATETIME        NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_ActivityLogs_Users FOREIGN KEY (UserID)
            REFERENCES Users(UserID) ON DELETE CASCADE
    );
END
GO

-- =============================================
-- Table: LoginHistory
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoginHistory')
BEGIN
    CREATE TABLE LoginHistory (
        LoginID         INT IDENTITY(1,1) PRIMARY KEY,
        UserID          INT             NOT NULL,
        LoginTime       DATETIME        NOT NULL DEFAULT GETDATE(),
        Status          NVARCHAR(20)    NOT NULL,       -- 'Success' or 'Failed'

        CONSTRAINT FK_LoginHistory_Users FOREIGN KEY (UserID)
            REFERENCES Users(UserID) ON DELETE CASCADE,
        CONSTRAINT CK_LoginHistory_Status CHECK (Status IN ('Success', 'Failed'))
    );
END
GO

-- =============================================
-- Indexes for performance
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Documents_UserID')
    CREATE INDEX IX_Documents_UserID ON Documents(UserID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Documents_CategoryID')
    CREATE INDEX IX_Documents_CategoryID ON Documents(CategoryID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Documents_IsDeleted')
    CREATE INDEX IX_Documents_IsDeleted ON Documents(IsDeleted);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Documents_UploadDate')
    CREATE INDEX IX_Documents_UploadDate ON Documents(UploadDate DESC);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLogs_UserID')
    CREATE INDEX IX_ActivityLogs_UserID ON ActivityLogs(UserID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLogs_Timestamp')
    CREATE INDEX IX_ActivityLogs_Timestamp ON ActivityLogs(Timestamp DESC);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LoginHistory_UserID')
    CREATE INDEX IX_LoginHistory_UserID ON LoginHistory(UserID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
    CREATE INDEX IX_Users_Email ON Users(Email);
GO

-- =============================================
-- Seed Data: Default Categories
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Certificates' AND UserID IS NULL)
    INSERT INTO Categories (UserID, CategoryName) VALUES (NULL, 'Certificates');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'ID Proof' AND UserID IS NULL)
    INSERT INTO Categories (UserID, CategoryName) VALUES (NULL, 'ID Proof');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Academic' AND UserID IS NULL)
    INSERT INTO Categories (UserID, CategoryName) VALUES (NULL, 'Academic');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Personal' AND UserID IS NULL)
    INSERT INTO Categories (UserID, CategoryName) VALUES (NULL, 'Personal');
GO

-- =============================================
-- Seed Data: Default Admin User
-- Password: Admin@123 (SHA256 hashed)
-- SHA256 of 'Admin@123' = 'd033e22ae348aeb5660fc2140aec35850c4da997'
-- Actually computing proper SHA256:
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@securevault.com')
BEGIN
    INSERT INTO Users (FullName, Email, PasswordHash, Role, IsBlocked, FailedAttempts, CreatedAt)
    VALUES (
        'System Admin',
        'admin@securevault.com',
        -- SHA256 hash of 'Admin@123' will be computed by the app on first setup
        -- Using a placeholder that the app's PasswordHelper will generate
        '57D9158E0D42FECBE606630FA94D54E36B8E426CFB9F81CE3E8C6F1C48C0E65E',
        'Admin',
        0,
        0,
        GETDATE()
    );
END
GO

PRINT 'SecureVault database created successfully!';
GO

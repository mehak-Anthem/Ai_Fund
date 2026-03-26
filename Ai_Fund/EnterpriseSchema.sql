-- Create ChatHistory table for user-based chat
CREATE TABLE ChatHistory (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(100) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);

-- Create AiLog table for tracking queries and responses
CREATE TABLE AiLog (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(100) NOT NULL,
    Query NVARCHAR(MAX) NOT NULL,
    Response NVARCHAR(MAX) NOT NULL,
    ConfidenceScore FLOAT NOT NULL,
    Intent NVARCHAR(50),
    Source NVARCHAR(50),
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);

-- Add versioning to MutualFundKnowledge
ALTER TABLE MutualFundKnowledge
ADD Version INT DEFAULT 1,
    IsActive BIT DEFAULT 1;

-- Create indexes for performance
CREATE INDEX IX_ChatHistory_UserId ON ChatHistory(UserId);
CREATE INDEX IX_ChatHistory_CreatedDate ON ChatHistory(CreatedDate);
CREATE INDEX IX_AiLog_UserId ON AiLog(UserId);
CREATE INDEX IX_AiLog_CreatedDate ON AiLog(CreatedDate);
-- Create Users table for authentication
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),

    Username NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Create index for performance
CREATE INDEX IX_Users_Username ON Users(Username);


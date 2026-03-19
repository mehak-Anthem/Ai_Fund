-- Create KnowledgeGaps table for tracking missing knowledge
CREATE TABLE KnowledgeGaps (
    Id INT IDENTITY PRIMARY KEY,
    Question NVARCHAR(MAX) NOT NULL,
    DetectedIntent NVARCHAR(100),
    ConfidenceScore FLOAT,
    OccurrenceCount INT DEFAULT 1,
    LastAsked DATETIME NOT NULL,
    Status NVARCHAR(50) DEFAULT 'New', -- New / Reviewing / Resolved
    SuggestedAnswer NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Create index for performance
CREATE INDEX IX_KnowledgeGaps_Status ON KnowledgeGaps(Status);
CREATE INDEX IX_KnowledgeGaps_LastAsked ON KnowledgeGaps(LastAsked DESC);
CREATE INDEX IX_KnowledgeGaps_OccurrenceCount ON KnowledgeGaps(OccurrenceCount DESC);

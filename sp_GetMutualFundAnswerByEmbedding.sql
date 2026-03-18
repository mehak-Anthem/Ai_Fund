-- Stored procedure for semantic search using embeddings
CREATE PROCEDURE sp_GetMutualFundAnswerByEmbedding
    @Embedding NVARCHAR(MAX)
AS
BEGIN
    -- This uses the existing Embedding column for vector similarity search
    -- Implement cosine similarity calculation between @Embedding and stored Embedding column
    
    SELECT TOP 1 Answer
    FROM MutualFundKnowledge
    WHERE Embedding IS NOT NULL
    -- Add your similarity calculation here
END

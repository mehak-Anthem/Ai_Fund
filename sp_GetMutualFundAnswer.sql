CREATE PROCEDURE sp_GetMutualFundAnswer
    @Query NVARCHAR(MAX)
AS
BEGIN
    SELECT TOP 1 Answer
    FROM MutualFundKnowledge
    WHERE Question LIKE '%' + @Query + '%'
END

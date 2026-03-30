namespace Ai_Fund.Data.Interfaces;

public interface IMutualFundRepository
{
    Task<List<(int Id, string Question, string Answer, string Embedding)>> GetAllKnowledgeAsync();
    Task UpdateEmbeddingAsync(int id, string embedding);
    Task<List<Models.ChatHistory>> GetChatHistoryAsync(string userId, int count = 5);
    Task SaveChatHistoryAsync(Models.ChatHistory chatHistory);
    Task SaveAiLogAsync(Models.AiLog aiLog);
    Task DeactivateKnowledgeAsync(int id);
    Task ActivateKnowledgeAsync(int id);
    Task UpdateKnowledgeVersionAsync(int id, int version);
    Task<Models.KnowledgeGap?> GetKnowledgeGapByQuestionAsync(string question);
    Task SaveKnowledgeGapAsync(Models.KnowledgeGap gap);
    Task UpdateKnowledgeGapAsync(Models.KnowledgeGap gap);
    Task<List<Models.KnowledgeGap>> GetTopKnowledgeGapsAsync(int count);
    Task AddKnowledgeFromGapAsync(string question, string answer);
    Task<int> GetAiLogCountAsync();
    Task<double> GetAverageConfidenceAsync();
    Task<int> GetActiveUserCountAsync(int days = 7);
    Task<int> GetUnansweredKnowledgeGapCountAsync();
    Task<List<(DateTime Date, int Count)>> GetDailyQueryCountsAsync(int days = 7);
    Task<List<(DateTime Date, double Value)>> GetDailyConfidenceTrendAsync(int days = 7);
    Task<List<(string Category, int Count)>> GetIntentCategoryUsageAsync(int top = 10);
    Task<List<(string Query, int Count, double AvgConfidence)>> GetTrendingQueriesAsync(int top = 10);
    Task<List<Models.KnowledgeGap>> GetKnowledgeGapsAsync(bool includeResolved = true, int top = 100);
    Task<Models.KnowledgeGap?> GetKnowledgeGapByIdAsync(int id);
}

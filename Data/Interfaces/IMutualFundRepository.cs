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
}

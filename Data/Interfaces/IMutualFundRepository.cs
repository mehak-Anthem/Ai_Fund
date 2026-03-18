namespace Ai_Fund.Data.Interfaces;

public interface IMutualFundRepository
{
    Task<List<(int Id, string Question, string Answer, string Embedding)>> GetAllKnowledgeAsync();
    Task UpdateEmbeddingAsync(int id, string embedding);
}

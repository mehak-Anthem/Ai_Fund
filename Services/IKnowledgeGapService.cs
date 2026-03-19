namespace Ai_Fund.Services;

public interface IKnowledgeGapService
{
    Task LogGapAsync(string question, string intent, double confidence);
    Task<List<Models.KnowledgeGap>> GetTopGapsAsync(int count = 10);
    Task ResolveGapAsync(int gapId, string answer);
}

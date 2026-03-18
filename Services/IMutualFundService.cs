namespace Ai_Fund.Services;

public interface IMutualFundService
{
    Task<string> GetAnswerAsync(string query);
    Task<string> GetAIAnswerAsync(string query);
}

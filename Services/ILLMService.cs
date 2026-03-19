using Ai_Fund.Models;

namespace Ai_Fund.Services;

public interface ILLMService
{
    Task<string> AskLLMAsync(string context, string query, List<ChatMessage> chatHistory, bool forceExpansion = false, bool isFollowUp = false, string? lastAnswer = null);
    Task<string> RewriteAnswerAsync(string answer, string query);
    Task<string> GenerateStructuredAsync(string prompt);
}

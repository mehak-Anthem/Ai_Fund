using Ai_Fund.Models;

namespace Ai_Fund.Services;

public interface ILLMService
{
    Task<string> AskLLMAsync(string context, string query, List<ChatMessage> chatHistory);
}

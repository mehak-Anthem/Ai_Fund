namespace Ai_Fund.Services;

public interface IStructuredAnswerService
{
    bool NeedsStructuredAnswer(string query);
    Task<string> GenerateStructuredAnswerAsync(string query, string context);
}

public class StructuredAnswerService : IStructuredAnswerService
{
    private readonly ILLMService _llmService;

    public StructuredAnswerService(ILLMService llmService)
    {
        _llmService = llmService;
    }

    public bool NeedsStructuredAnswer(string query)
    {
        query = query.ToLower();

        return query.Contains("how much") ||
               query.Contains("how to invest") ||
               query.Contains("how should i invest") ||
               query.Contains("how should invest") ||
               query.Contains("what amount") ||
               query.Contains("how to start") ||
               query.Contains("how do i start") ||
               query.Contains("best") ||
               query.Contains("top") ||
               query.Contains("recommend") ||
               query.Contains("suggest") ||
               query.Contains("which fund") ||
               query.Contains("which mutual fund") ||
               (query.Contains("invest") && query.Contains("beginner")) ||
               (query.Contains("invest") && query.Contains("bignner")) ||
               query.Contains("plan") ||
               query.Contains("strategy") ||
               query.Contains("allocate") ||
               query.Contains("percentage") ||
               (query.Contains("2024") || query.Contains("2025"));
    }

    public async Task<string> GenerateStructuredAnswerAsync(string query, string context)
    {
        var prompt = $@"
You are Miria, a knowledgeable financial assistant.

User Question: {query}

Knowledge Base Context:
{context}

Provide a well-structured, informative answer based on the context.

FORMAT:
1. Start with a clear, direct answer
2. Provide 2-4 key points with bullet points or numbers
3. Include practical examples with realistic numbers when relevant
4. End with actionable advice or next steps

STYLE:
- Professional yet friendly
- Clear and confident
- Evidence-based (use the context provided)
- 5-10 lines maximum
- Use emojis sparingly for readability

IMPORTANT:
- Base your answer on the provided context
- Give general guidance, not personal advice
- Be specific about fund categories, not individual fund names
- Include realistic return expectations with market risk disclaimers
- Do NOT repeat the question
- Do NOT include these instructions in your answer
- Do NOT introduce yourself or your role (e.g., never start with ""As a knowledgeable financial assistant""). Just answer directly.

Answer:";

        return await _llmService.GenerateStructuredAsync(prompt);
    }
}

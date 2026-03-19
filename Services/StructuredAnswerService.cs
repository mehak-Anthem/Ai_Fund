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
               (query.Contains("invest") && query.Contains("beginner")) ||
               (query.Contains("invest") && query.Contains("bignner")) ||
               query.Contains("plan") ||
               query.Contains("strategy") ||
               query.Contains("allocate") ||
               query.Contains("percentage");
    }

    public async Task<string> GenerateStructuredAnswerAsync(string query, string context)
    {
        var prompt = $@"
You are Miria, a smart financial assistant.

Answer the question in a structured, practical, and easy-to-understand way.

STRICT FORMAT:

1. Start with a clear, honest statement (no generic disclaimer)
2. Give a simple rule (percent or idea)
3. Give 2–3 practical examples (numbers)
4. Give goal-based thinking (real-world guidance)

STYLE:
- Friendly but professional
- Clear and confident
- 4–8 lines max
- Use bullet points where helpful

IMPORTANT:
- Do NOT say ""I cannot give advice""
- Give general guidance only
- Do NOT repeat sentences

Context:
{context}

User Question:
{query}

Answer:";

        return await _llmService.GenerateStructuredAsync(prompt);
    }
}

namespace Ai_Fund.Services;

public interface IRewriteService
{
    Task<string> RewriteAnswerAsync(string answer, string query);
}

public class RewriteService : IRewriteService
{
    private readonly ILLMService _llm;

    public RewriteService(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<string> RewriteAnswerAsync(string answer, string query)
    {
        var prompt = $@"You are a professional financial assistant.

Rewrite the answer below in a clear, natural, and conversational tone.

Rules:
- Keep the meaning EXACTLY the same
- Do NOT add new information
- Keep it short (2–3 sentences)
- Make it sound human and confident
- Avoid repetition
- Keep financial disclaimer if present

Question: {query}

Answer: {answer}

Rewritten Answer:";

        return await _llm.RewriteAnswerAsync(answer, query);
    }
}

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
        // Skip rewriting if answer is already clean and short
        if (answer.Length < 200 && !answer.Contains("However") && !answer.Contains("Therefore"))
        {
            return answer;
        }

        var rewritten = await _llm.RewriteAnswerAsync(answer, query);
        
        // If rewrite failed or added artifacts, return original
        if (string.IsNullOrWhiteSpace(rewritten) || 
            rewritten.Contains("Sure!") ||
            rewritten.Contains("Here's") ||
            rewritten.Contains("updated version") ||
            rewritten.Length > answer.Length * 1.5)
        {
            return answer;
        }
        
        return rewritten;
    }
}

using System.Text.RegularExpressions;

namespace Ai_Fund.Services;

public interface ISmartGuidanceService
{
    bool IsPersonalQuery(string query);
    Task<string> GenerateGuidedAnswerAsync(string query, string context);
    int ExtractAmount(string query);
    string GenerateInvestmentAdvice(int amount);
    bool IsReturnsQuery(string query);
    string GenerateReturnsGuidance();
}

public class SmartGuidanceService : ISmartGuidanceService
{
    private readonly ILLMService _llmService;

    public SmartGuidanceService(ILLMService llmService)
    {
        _llmService = llmService;
    }

    public bool IsPersonalQuery(string query)
    {
        query = query.ToLower();

        return query.Contains("i have") ||
               query.Contains("my money") ||
               query.Contains("should i invest") ||
               query.Contains("i should invest") ||
               query.Contains("where should i") ||
               query.Contains("where i should") ||
               query.Contains("how much should i") ||
               query.Contains("how much i should") ||
               query.Contains("what should i do") ||
               query.Contains("i want to invest") ||
               (query.Contains("i") && query.Contains("invest") && query.Contains("where")) ||
               (query.Contains("i") && query.Contains("invest") && query.Contains("how much"));
    }

    public int ExtractAmount(string query)
    {
        var match = Regex.Match(query, @"\d+");
        return match.Success ? int.Parse(match.Value) : 0;
    }

    public bool IsReturnsQuery(string query)
    {
        query = query.ToLower();
        return query.Contains("return") || query.Contains("profit") || query.Contains("gain");
    }

    public string GenerateReturnsGuidance()
    {
        return @"Returns depend on market conditions, but here's a realistic idea:

📊 Example:
₹5,000/month SIP for 10 years → ~₹10-12 lakh (approx)

(assuming ~12% annual return)

🎯 Tip:
Long-term investing gives better results than short-term.";
    }

    public string GenerateInvestmentAdvice(int amount)
    {
        if (amount >= 10000)
        {
            var half = amount / 2;
            return $@"Good question. With ₹{amount:N0}, you can balance safety and growth.

💡 Simple approach:
• ₹{half:N0} in FD → safe, stable returns
• ₹{half:N0} in SIP → growth over time

🎯 Tip:
Use FD for short-term needs, and SIP for long-term wealth building.";
        }
        else if (amount >= 5000)
        {
            return $@"With ₹{amount:N0}, SIP is a good starting option for long-term growth.

💡 Simple idea:
• Start with ₹{amount:N0}/month SIP
• Stay invested for 5-10 years

🎯 Tip:
Consistency matters more than amount in SIP.";
        }
        else if (amount > 0)
        {
            return $@"With ₹{amount:N0}, you can start small with SIP.

💡 Simple approach:
• Begin with ₹{amount:N0}/month
• Increase gradually as income grows

🎯 Tip:
Starting early is more important than starting big.";
        }

        return string.Empty;
    }

    public async Task<string> GenerateGuidedAnswerAsync(string query, string context)
    {
        var prompt = $@"
You are Miria, a smart financial assistant.

User asked: {query}

Context: {context}

Give a helpful, practical answer in 4-6 lines.

RULES:
- Start with Good question. It depends on your goals/risk level/time horizon
- Give 2-3 options with bullet points
- Include ONE simple example with numbers
- End with a practical tip
- Do NOT include prompt instructions in your answer
- Do NOT say I cannot give advice

Answer directly without repeating these instructions:";

        return await _llmService.GenerateStructuredAsync(prompt);
    }
}

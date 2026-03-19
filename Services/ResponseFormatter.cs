namespace Ai_Fund.Services;

public static class ResponseFormatter
{
    public static string FormatResponse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // Remove repetitions
        text = text.Replace("Yes, Yes,", "Yes,");
        text = text.Replace("No, No,", "No,");
        text = text.Replace("So, So,", "So,");
        
        // Remove multiple spaces
        while (text.Contains("  "))
        {
            text = text.Replace("  ", " ");
        }

        // Limit to 2-3 sentences
        var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var cleanSentences = sentences
            .Select(s => s.Trim())
            .Where(s => s.Length > 10) // Keep only substantial sentences
            .Take(3);

        if (!cleanSentences.Any())
            return text;

        var result = string.Join(". ", cleanSentences).Trim();
        
        // Ensure it ends with punctuation
        if (!result.EndsWith(".") && !result.EndsWith("!") && !result.EndsWith("?"))
        {
            result += ".";
        }

        return result;
    }

    public static bool IsComparisonQuery(string query)
    {
        query = query.ToLower();
        return query.Contains(" vs ") ||
               query.Contains(" versus ") ||
               query.Contains("better") ||
               query.Contains("difference between") ||
               query.Contains("compare");
    }

    public static bool IsGuidanceQuery(string query)
    {
        query = query.ToLower();
        return query.Contains("should i") ||
               query.Contains("what do you think") ||
               query.Contains("your opinion") ||
               query.Contains("recommend") ||
               query.Contains("suggest");
    }

    public static string HandleComparison(string query)
    {
        // Check for SIP vs Mutual Fund confusion
        if ((query.Contains("sip", StringComparison.OrdinalIgnoreCase) && 
             query.Contains("mutual fund", StringComparison.OrdinalIgnoreCase)) ||
            (query.Contains("sip", StringComparison.OrdinalIgnoreCase) && 
             query.Contains("fund", StringComparison.OrdinalIgnoreCase)))
        {
            return "SIP is a way of investing regularly, while mutual funds are the actual investment products. SIP helps in disciplined investing, whereas mutual funds provide diversification and professional management. Both are related, as SIP is commonly used to invest in mutual funds.";
        }

        return string.Empty;
    }

    public static string HandleGuidance(string query)
    {
        return "I can't give personal financial advice, but generally SIP is suitable for long-term investing and disciplined wealth building. It depends on your goals and risk tolerance. Mutual funds are subject to market risk.";
    }
}

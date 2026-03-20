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
        
        // Allow informational queries about best/top funds
        if (query.Contains("best") || query.Contains("top") || 
            query.Contains("2024") || query.Contains("2025") ||
            query.Contains("mutual fund") || query.Contains("sip"))
        {
            return false;
        }
        
        // Only block personal advice requests
        return query.Contains("should i") ||
               query.Contains("what do you think") ||
               query.Contains("your opinion");
    }

    // Removed static comparison handler - now uses dynamic RAG+LLM
    
    // Removed static guidance handler - now uses dynamic RAG+LLM
}

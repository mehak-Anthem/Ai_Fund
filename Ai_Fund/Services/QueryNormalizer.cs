namespace Ai_Fund.Services;

public static class QueryNormalizer
{
    public static string NormalizeQuery(string query)
    {
        query = query.ToLower().Trim();

        // Remove opinion-seeking phrases
        query = query.Replace("what u think", "");
        query = query.Replace("what you think", "");
        query = query.Replace("your opinion", "");
        query = query.Replace("tell me", "");
        query = query.Replace("can you", "");
        
        // Clean up extra spaces
        while (query.Contains("  "))
        {
            query = query.Replace("  ", " ");
        }

        return query.Trim();
    }

    public static bool IsOpinionQuery(string query)
    {
        query = query.ToLower();
        
        // Allow legitimate informational queries about best/top funds
        if (query.Contains("best") || query.Contains("top") || 
            query.Contains("mutual fund") || query.Contains("sip") ||
            query.Contains("investment") || query.Contains("scheme"))
        {
            return false;
        }
        
        // Block only personal opinion requests
        return (query.Contains("what do you think") || 
                query.Contains("what u think") ||
                query.Contains("your opinion") ||
                query.Contains("how do you feel"));
    }
}

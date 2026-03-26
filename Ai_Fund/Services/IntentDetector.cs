namespace Ai_Fund.Services;

public static class IntentDetector
{
    public static string DetectIntent(string query)
    {
        query = query.ToLower();

        // 0. GREETING - "how are you" variations
        if (query.Contains("how are you") || query.Contains("how are u") || 
            query.Contains("how r you") || query.Contains("how r u"))
            return "GREETING";
        
        // 0.5. MF_SPECIFIC (high priority)
        if (query.Contains("nav") || query.Contains("latest price") || query.Contains("current price") || 
            (query.Contains("how") && query.Contains("doing") && (query.Contains("fund") || query.Contains("scheme"))))
            return "MF_SPECIFIC";
        
        // 0.6. CURRENCY (high priority)
        if (query.Contains("exchange rate") || query.Contains("usd rate") || query.Contains("dollar rate") || query.Contains("currency") ||
            (query.Contains("rate") && (query.Contains("usa") || query.Contains("us ") || query.Contains("dollar") || query.Contains("today") || query.Contains("now"))) ||
            (query.Contains("india") && query.Contains("usa") && (query.Contains("today") || query.Contains("rate"))))
            return "CURRENCY";



        // 1. COMPARISON (high priority)
        if (query.Contains("difference") || query.Contains("diff") || query.Contains(" vs ") || 
            query.Contains("versus") || query.Contains("compare") || 
            (query.Contains("between") && query.Contains(" and ")))
            return "COMPARISON";

        // 2. QUESTION should be highest priority
        if (query.Contains("what") || query.Contains("is") || query.Contains("how") || 
            query.Contains("why") || query.Contains("when") || query.Contains("where"))
            return "QUESTION";

        // 3. ADVICE
        if (query.Contains("should") || query.Contains("best") || query.Contains("good") || query.Contains("recommend"))
            return "ADVICE";

        // 4. GREETING (exact match only - LOW priority)
        if (query.Trim() == "hi" || query.Trim() == "hello" || query.Trim() == "hey")
            return "GREETING";

        // 5. CLOSING
        if (query.Contains("thank") || query.Contains("bye"))
            return "CLOSING";

        return "GENERAL";
    }
}

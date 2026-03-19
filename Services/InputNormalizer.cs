namespace Ai_Fund.Services;

public static class InputNormalizer
{
    private static readonly Dictionary<string, string> CommonTypos = new()
    {
        // Greetings
        { "gi", "hi" },
        { "helo", "hello" },
        { "hii", "hi" },
        { "hiii", "hi" },
        
        // Common words
        { "wat", "what" },
        { "wht", "what" },
        { "hw", "how" },
        { "abt", "about" },
        { "plz", "please" },
        { "pls", "please" },
        { "thnks", "thanks" },
        { "thx", "thanks" },
        { "u", "you" },
        { "ur", "your" },
        { "r", "are" },
        
        // Financial terms
        { "sipp", "sip" },
        { "mutal", "mutual" },
        { "mutaul", "mutual" },
        { "fundd", "fund" },
        { "invesment", "investment" },
        { "investmnt", "investment" }
    };

    public static string NormalizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var normalized = input.Trim();
        var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < words.Length; i++)
        {
            var lowerWord = words[i].ToLower();
            if (CommonTypos.ContainsKey(lowerWord))
            {
                words[i] = CommonTypos[lowerWord];
            }
        }
        
        return string.Join(" ", words);
    }

    public static bool IsUselessQuery(string query, double confidence)
    {
        // Ignore very short queries with low confidence
        if (confidence < 0.3 && query.Length < 3)
            return true;

        // Ignore single character queries
        if (query.Trim().Length == 1)
            return true;

        // Ignore queries with only special characters
        if (query.All(c => !char.IsLetterOrDigit(c)))
            return true;

        return false;
    }
}

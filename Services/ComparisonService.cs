namespace Ai_Fund.Services;

public interface IComparisonService
{
    bool IsComparisonQuery(string query);
    (string Entity1, string Entity2)? ExtractEntities(string query);
    List<string> ExtractAllEntities(string query);
    string ResolveComparison(string query, string userId, IContextManager contextManager);
}

public class ComparisonService : IComparisonService
{
    public bool IsComparisonQuery(string query)
    {
        query = query.ToLower();

        return query.Contains("difference") ||
               query.Contains("diff") ||
               query.Contains("vs") ||
               query.Contains("versus") ||
               query.Contains("compare") ||
               query.Contains("comparison") ||
               query.Contains("better") ||
               (query.Contains("between") && query.Contains("and"));
    }

    public List<string> ExtractAllEntities(string query)
    {
        var entities = new List<string>();
        query = query.ToLower();

        // Check for specific financial entities
        if (query.Contains("sip")) entities.Add("SIP");
        if (query.Contains("mutual fund")) entities.Add("Mutual Fund");
        if (query.Contains("fd") || query.Contains("fixed deposit")) entities.Add("Fixed Deposit");
        if (query.Contains("stock") || query.Contains("equity")) entities.Add("Stock");
        if (query.Contains("bond")) entities.Add("Bond");
        if (query.Contains("etf")) entities.Add("ETF");
        if (query.Contains("ppf")) entities.Add("PPF");
        if (query.Contains("nps")) entities.Add("NPS");
        if (query.Contains("gold")) entities.Add("Gold");
        if (query.Contains("real estate") || query.Contains("property")) entities.Add("Real Estate");

        return entities.Distinct().ToList();
    }

    public (string Entity1, string Entity2)? ExtractEntities(string query)
    {
        // First try to extract all entities
        var allEntities = ExtractAllEntities(query);
        
        // If we have at least 2, return first two for backward compatibility
        if (allEntities.Count >= 2)
        {
            return (allEntities[0], allEntities[1]);
        }

        query = query.ToLower();

        // Pattern: "difference between X and Y"
        var betweenIndex = query.IndexOf("between");
        var andIndex = query.IndexOf(" and ");

        if (betweenIndex >= 0 && andIndex > betweenIndex)
        {
            var entity1Start = betweenIndex + "between".Length;
            var entity1 = query.Substring(entity1Start, andIndex - entity1Start).Trim();

            var entity2Start = andIndex + " and ".Length;
            var entity2End = query.IndexOfAny(new[] { '?', '.' }, entity2Start);
            var entity2 = entity2End > 0 
                ? query.Substring(entity2Start, entity2End - entity2Start).Trim()
                : query.Substring(entity2Start).Trim();

            if (!string.IsNullOrEmpty(entity1) && !string.IsNullOrEmpty(entity2))
            {
                return (NormalizeEntity(entity1), NormalizeEntity(entity2));
            }
        }

        // Pattern: "X vs Y"
        var vsIndex = query.IndexOf(" vs ");
        if (vsIndex < 0) vsIndex = query.IndexOf(" versus ");

        if (vsIndex >= 0)
        {
            var separator = query.Contains(" vs ") ? " vs " : " versus ";
            var parts = query.Split(new[] { separator }, StringSplitOptions.None);
            
            if (parts.Length == 2)
            {
                var entity1 = parts[0].Trim();
                var entity2 = parts[1].Replace("?", "").Trim();
                
                return (NormalizeEntity(entity1), NormalizeEntity(entity2));
            }
        }

        return null;
    }

    public string ResolveComparison(string query, string userId, IContextManager contextManager)
    {
        query = query.ToLower();

        // Handle "both" reference
        if (query.Contains("both"))
        {
            var lastEntities = contextManager.GetLastEntities(userId);
            if (lastEntities.HasValue)
            {
                return $"difference between {lastEntities.Value.Entity1} and {lastEntities.Value.Entity2}";
            }
        }

        // Handle "difference between them"
        if ((query.Contains("difference") || query.Contains("compare")) && 
            (query.Contains("them") || query.Contains("these") || query.Contains("those")))
        {
            var lastEntities = contextManager.GetLastEntities(userId);
            if (lastEntities.HasValue)
            {
                return $"difference between {lastEntities.Value.Entity1} and {lastEntities.Value.Entity2}";
            }
        }

        return query;
    }

    private string NormalizeEntity(string entity)
    {
        entity = entity.Trim();
        
        // Remove common words
        var removeWords = new[] { "what", "is", "the", "a", "an" };
        foreach (var word in removeWords)
        {
            entity = entity.Replace($"{word} ", "");
        }

        // Capitalize properly
        if (entity.ToLower() == "sip")
            return "SIP";
        
        if (entity.ToLower().Contains("mutual fund"))
            return "Mutual Fund";

        if (entity.ToLower().Contains("nav"))
            return "NAV";

        // Title case for others
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(entity);
    }
}

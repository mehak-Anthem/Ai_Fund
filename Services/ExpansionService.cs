namespace Ai_Fund.Services;

public interface IExpansionService
{
    bool IsExpansionQuery(string query);
    string ExpandQuery(string query, string topic);
    string GetExpansionPrompt();
}

public class ExpansionService : IExpansionService
{
    public bool IsExpansionQuery(string query)
    {
        query = query.ToLower();

        return query.Contains("more") ||
               query.Contains("details") ||
               query.Contains("explain more") ||
               query.Contains("tell me more") ||
               query.Contains("elaborate") ||
               query.Contains("deeper") ||
               query.Contains("in detail");
    }

    public string ExpandQuery(string query, string topic)
    {
        if (string.IsNullOrEmpty(topic))
            return query;

        return $"Explain {topic} in detail with benefits, risks, and how it works";
    }

    public string GetExpansionPrompt()
    {
        return "\nGive more detailed explanation including benefits, risks, and examples.";
    }
}

namespace Ai_Fund.Services;

public interface IContextManager
{
    void SetLastTopic(string userId, string topic, string intent);
    (string Topic, string Intent) GetLastContext(string userId);
    bool IsFollowUpQuery(string query);
    string ResolveFollowUp(string query, string userId);
}

public class ContextManager : IContextManager
{
    private readonly Dictionary<string, (string Topic, string Intent, DateTime Timestamp)> _userContexts = new();
    private readonly TimeSpan _contextTimeout = TimeSpan.FromMinutes(10);

    public void SetLastTopic(string userId, string topic, string intent)
    {
        _userContexts[userId] = (topic, intent, DateTime.UtcNow);
    }

    public (string Topic, string Intent) GetLastContext(string userId)
    {
        if (_userContexts.TryGetValue(userId, out var context))
        {
            // Check if context is still valid (within timeout)
            if (DateTime.UtcNow - context.Timestamp < _contextTimeout)
            {
                return (context.Topic, context.Intent);
            }
            
            // Context expired, remove it
            _userContexts.Remove(userId);
        }
        
        return (string.Empty, string.Empty);
    }

    public bool IsFollowUpQuery(string query)
    {
        query = query.ToLower().Trim();
        
        // Check for follow-up indicators
        return query.Contains("more") ||
               query.Contains("it") ||
               query.Contains("that") ||
               query.Contains("this") ||
               query.Contains("explain") ||
               query.Contains("tell me more") ||
               query.Contains("what about") ||
               query.Contains("how about") ||
               query.Length < 5;
    }

    public string ResolveFollowUp(string query, string userId)
    {
        if (!IsFollowUpQuery(query))
            return query;

        var (lastTopic, _) = GetLastContext(userId);
        
        if (!string.IsNullOrEmpty(lastTopic))
        {
            // Inject context into query
            return $"{lastTopic} {query}";
        }
        
        return query;
    }
}

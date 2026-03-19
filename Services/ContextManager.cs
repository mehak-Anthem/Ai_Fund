namespace Ai_Fund.Services;

public interface IContextManager
{
    void SetLastTopic(string userId, string topic, string intent);
    (string Topic, string Intent) GetLastContext(string userId);
    bool IsFollowUpQuery(string query);
    string ResolveFollowUp(string query, string userId);
    void SaveContext(string userId, string topic, string intent, string lastQuestion);
    string GetLastQuestion(string userId);
    string GetLastTopic(string userId);
    void SaveLastAnswer(string userId, string answer);
    string GetLastAnswer(string userId);
    void SaveLastEntities(string userId, string entity1, string entity2);
    (string Entity1, string Entity2)? GetLastEntities(string userId);
    void SaveRichContext(string userId, string userQuery, string answer, string topic, List<string> entities);
    ConversationContext? GetRichContext(string userId);
}

public class ContextManager : IContextManager
{
    private readonly Dictionary<string, ConversationContext> _userContexts = new();
    private readonly TimeSpan _contextTimeout = TimeSpan.FromMinutes(10);

    public void SetLastTopic(string userId, string topic, string intent)
    {
        if (!_userContexts.ContainsKey(userId))
        {
            _userContexts[userId] = new ConversationContext();
        }
        
        _userContexts[userId].LastTopic = topic;
        _userContexts[userId].LastIntent = intent;
        _userContexts[userId].Timestamp = DateTime.UtcNow;
    }

    public void SaveContext(string userId, string topic, string intent, string lastQuestion)
    {
        _userContexts[userId] = new ConversationContext
        {
            LastTopic = topic,
            LastIntent = intent,
            LastQuestion = lastQuestion,
            Timestamp = DateTime.UtcNow
        };
    }

    public string GetLastTopic(string userId)
    {
        if (_userContexts.TryGetValue(userId, out var context))
        {
            if (DateTime.UtcNow - context.Timestamp < _contextTimeout)
            {
                return context.LastTopic ?? string.Empty;
            }
        }
        return string.Empty;
    }

    public void SaveLastAnswer(string userId, string answer)
    {
        if (_userContexts.ContainsKey(userId))
        {
            _userContexts[userId].LastAnswer = answer;
            _userContexts[userId].Timestamp = DateTime.UtcNow;
        }
    }

    public string GetLastAnswer(string userId)
    {
        if (_userContexts.TryGetValue(userId, out var context))
        {
            if (DateTime.UtcNow - context.Timestamp < _contextTimeout)
            {
                return context.LastAnswer ?? string.Empty;
            }
        }
        return string.Empty;
    }

    public void SaveLastEntities(string userId, string entity1, string entity2)
    {
        if (_userContexts.ContainsKey(userId))
        {
            _userContexts[userId].LastEntity1 = entity1;
            _userContexts[userId].LastEntity2 = entity2;
            _userContexts[userId].Timestamp = DateTime.UtcNow;
        }
        else
        {
            _userContexts[userId] = new ConversationContext
            {
                LastEntity1 = entity1,
                LastEntity2 = entity2,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public (string Entity1, string Entity2)? GetLastEntities(string userId)
    {
        if (_userContexts.TryGetValue(userId, out var context))
        {
            if (DateTime.UtcNow - context.Timestamp < _contextTimeout)
            {
                if (!string.IsNullOrEmpty(context.LastEntity1) && !string.IsNullOrEmpty(context.LastEntity2))
                {
                    return (context.LastEntity1, context.LastEntity2);
                }
            }
        }
        return null;
    }

    public void SaveRichContext(string userId, string userQuery, string answer, string topic, List<string> entities)
    {
        _userContexts[userId] = new ConversationContext
        {
            LastUserQuery = userQuery,
            LastAnswer = answer,
            LastTopic = topic,
            LastEntities = entities ?? new List<string>(),
            LastEntity1 = entities?.Count > 0 ? entities[0] : string.Empty,
            LastEntity2 = entities?.Count > 1 ? entities[1] : string.Empty,
            Timestamp = DateTime.UtcNow
        };
    }

    public ConversationContext? GetRichContext(string userId)
    {
        if (_userContexts.TryGetValue(userId, out var context))
        {
            if (DateTime.UtcNow - context.Timestamp < _contextTimeout)
            {
                return context;
            }
            _userContexts.Remove(userId);
        }
        return null;
    }

    public string GetLastQuestion(string userId)
    {
        if (_userContexts.TryGetValue(userId, out var context))
        {
            if (DateTime.UtcNow - context.Timestamp < _contextTimeout)
            {
                return context.LastQuestion ?? string.Empty;
            }
        }
        return string.Empty;
    }

    public (string Topic, string Intent) GetLastContext(string userId)
    {
        if (_userContexts.TryGetValue(userId, out var context))
        {
            if (DateTime.UtcNow - context.Timestamp < _contextTimeout)
            {
                return (context.LastTopic, context.LastIntent);
            }
            
            _userContexts.Remove(userId);
        }
        
        return (string.Empty, string.Empty);
    }

    public bool IsFollowUpQuery(string query)
    {
        query = query.ToLower().Trim();
        
        // Short queries are likely follow-ups
        if (query.Length < 20)
            return true;
        
        // Single word follow-ups
        if (query == "why" || query == "how" || query == "more" || query == "what" || query == "when")
            return true;
        
        // Check for vague reference words
        return query.Contains("it") ||
               query.Contains("that") ||
               query.Contains("this") ||
               query.Contains("more") ||
               query.Contains("then") ||
               query.Contains("explain") ||
               query.Contains("tell me more") ||
               query.Contains("what about") ||
               query.Contains("how about");
    }

    public string ResolveFollowUp(string query, string userId)
    {
        if (!IsFollowUpQuery(query))
            return query;

        var context = GetRichContext(userId);
        
        if (context != null && !string.IsNullOrEmpty(context.LastTopic))
        {
            // Attach topic to vague query for better RAG retrieval
            return $"{context.LastTopic} - {query}";
        }
        
        return query;
    }
}

public class ConversationContext
{
    public string LastUserQuery { get; set; } = string.Empty;
    public string LastAnswer { get; set; } = string.Empty;
    public string LastTopic { get; set; } = string.Empty;
    public List<string> LastEntities { get; set; } = new List<string>();
    public string LastIntent { get; set; } = string.Empty;
    public string LastQuestion { get; set; } = string.Empty;
    public string LastEntity1 { get; set; } = string.Empty;
    public string LastEntity2 { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

using Ai_Fund.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Ai_Fund.Services;

public class GroqLLMService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly IPersonalityService _personalityService;
    private readonly ILogger<GroqLLMService> _logger;

    public GroqLLMService(IConfiguration configuration, IPersonalityService personalityService, ILogger<GroqLLMService> logger)
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(2);
        
        var apiKey = configuration["Groq:ApiKey"] ?? throw new InvalidOperationException("Groq:ApiKey is required");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        
        _model = configuration["Groq:Model"] ?? "llama-3.1-8b-instant";
        _personalityService = personalityService;
        _logger = logger;
        
        _logger.LogInformation("GroqLLMService initialized with model: {Model}", _model);
    }

    public async Task<string> AskLLMAsync(string context, string query, List<ChatMessage> chatHistory, bool forceExpansion = false, bool isFollowUp = false, string? lastAnswer = null)
    {
        var systemPrompt = $@"{_personalityService.GetPersonalityPrompt()}

GUIDELINES:
- Speak DIRECTLY to the user. Do NOT describe what you are doing.
- Do NOT introduce your role or yourself.
- Use the context to give factual, accurate information
- Be conversational and friendly
- Keep it concise (3-5 sentences) unless more detail is requested
- For 'best' or 'top' queries, mention fund categories and characteristics
- For comparison queries, highlight key differences
- Include practical examples when relevant
- Always mention 'mutual funds are subject to market risk' when discussing returns";

        if (forceExpansion)
            systemPrompt += "\n\nProvide detailed explanation including benefits, risks, examples, and practical tips.";

        if (isFollowUp)
            systemPrompt += "\n\nProvide additional details and avoid repeating the previous answer.";

        if (!string.IsNullOrEmpty(lastAnswer) && context.Contains(lastAnswer))
            systemPrompt += "\n\nGive deeper explanation with new points not mentioned before.";

        var userMessage = $@"Knowledge Base Context:
{context}

User Question: {query}

Provide an accurate, helpful answer based on the context above.";

        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        // Add chat history
        foreach (var msg in chatHistory)
        {
            messages.Add(new { role = msg.Role.ToLower(), content = msg.Content });
        }

        messages.Add(new { role = "user", content = userMessage });

        return await CallGroqAsync(messages, 0.1, forceExpansion ? 250 : 150);
    }

    public async Task<string> RewriteAnswerAsync(string answer, string query)
    {
        var messages = new List<object>
        {
            new { role = "system", content = "You are a helpful rewriting assistant. Rewrite the given text concisely." },
            new { role = "user", content = $"Rewrite this in 2-3 simple sentences:\n\n{answer}\n\nRewritten:" }
        };

        return await CallGroqAsync(messages, 0.1, 150);
    }

    public async Task<string> GenerateStructuredAsync(string prompt)
    {
        var messages = new List<object>
        {
            new { role = "system", content = "You are a helpful assistant that generates structured responses." },
            new { role = "user", content = prompt }
        };

        return await CallGroqAsync(messages, 0.2, 300);
    }

    private async Task<string> CallGroqAsync(List<object> messages, double temperature, int maxTokens)
    {
        var request = new
        {
            model = _model,
            messages = messages,
            temperature = temperature,
            max_tokens = maxTokens
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("https://api.groq.com/openai/v1/chat/completions", request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var content = json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return content ?? "I couldn't generate a response.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Groq API");
            return "I'm having trouble connecting to the AI service. Please try again.";
        }
    }
}

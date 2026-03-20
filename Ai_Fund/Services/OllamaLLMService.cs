using Ai_Fund.Models;

namespace Ai_Fund.Services;

public class OllamaLLMService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaEndpoint;
    private readonly string _model;
    private readonly IPersonalityService _personalityService;

    public OllamaLLMService(IConfiguration configuration, IPersonalityService personalityService)
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(2);
        _ollamaEndpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
        _model = configuration["Ollama:Model"] ?? "tinyllama";
        _personalityService = personalityService;
    }

    public async Task<string> AskLLMAsync(string context, string query, List<ChatMessage> chatHistory, bool forceExpansion = false, bool isFollowUp = false, string? lastAnswer = null)
    {
        var historyText = string.Join("\n", chatHistory
            .Select(x => $"{x.Role}: {x.Content}"));

        var prompt = $@"{_personalityService.GetPersonalityPrompt()}

Knowledge Base Context:
{context}

User Question: {query}

Provide an accurate, helpful answer based on the context above.

GUIDELINES:
- Speak DIRECTLY to the user. Do NOT describe what you are doing (e.g., never say ""Responding to a user's query..."").
- Do NOT introduce your role or yourself (e.g., never say ""As an AI assistant..."").
- Use the context to give factual, accurate information
- Be conversational and friendly
- Keep it concise (3-5 sentences) unless more detail is requested
- For 'best' or 'top' queries, mention fund categories and characteristics
- For comparison queries, highlight key differences
- Include practical examples when relevant
- Always mention 'mutual funds are subject to market risk' when discussing returns";

        if (forceExpansion)
        {
            prompt += "\n\nProvide detailed explanation including benefits, risks, examples, and practical tips.";
        }

        if (isFollowUp)
        {
            prompt += "\n\nProvide additional details and avoid repeating the previous answer.";
        }

        if (!string.IsNullOrEmpty(lastAnswer) && context.Contains(lastAnswer))
        {
            prompt += "\n\nGive deeper explanation with new points not mentioned before.";
        }

        prompt += "\n\nAnswer:";

        var request = new
        {
            model = _model,
            prompt = prompt,
            stream = false,
            options = new
            {
                temperature = 0.1,
                top_p = 0.9,
                max_tokens = forceExpansion ? 250 : 150
            }
        };

        var response = await _httpClient.PostAsJsonAsync($"{_ollamaEndpoint}/api/generate", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();

        return result?.response ?? "I couldn't generate a response.";
    }

    public async Task<string> RewriteAnswerAsync(string answer, string query)
    {
        var prompt = $@"Rewrite this in 2-3 simple sentences:

{answer}

Rewritten:";

        var request = new
        {
            model = _model,
            prompt = prompt,
            stream = false,
            options = new
            {
                temperature = 0.1,
                top_p = 0.9,
                max_tokens = 150
            }
        };

        var response = await _httpClient.PostAsJsonAsync($"{_ollamaEndpoint}/api/generate", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();

        return result?.response ?? answer;
    }

    public async Task<string> GenerateStructuredAsync(string prompt)
    {
        var request = new
        {
            model = _model,
            prompt = prompt,
            stream = false,
            options = new
            {
                temperature = 0.2,
                top_p = 0.9,
                max_tokens = 300
            }
        };

        var response = await _httpClient.PostAsJsonAsync($"{_ollamaEndpoint}/api/generate", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();

        return result?.response ?? "I couldn't generate a structured response.";
    }
}

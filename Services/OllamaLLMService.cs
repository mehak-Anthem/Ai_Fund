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

    public async Task<string> AskLLMAsync(string context, string query, List<ChatMessage> chatHistory, bool forceExpansion = false)
    {
        var historyText = string.Join("\n", chatHistory
            .Select(x => $"{x.Role}: {x.Content}"));

        var prompt = $@"{_personalityService.GetPersonalityPrompt()}

Context: {context}

Question: {query}";

        if (forceExpansion)
        {
            prompt += "\n\nGive more detailed explanation including benefits, risks, and examples.";
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
}

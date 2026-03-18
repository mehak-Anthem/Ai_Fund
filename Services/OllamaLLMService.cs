using Ai_Fund.Models;

namespace Ai_Fund.Services;

public class OllamaLLMService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaEndpoint;
    private readonly string _model;

    public OllamaLLMService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(2);
        _ollamaEndpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
        _model = configuration["Ollama:Model"] ?? "tinyllama";
    }

    public async Task<string> AskLLMAsync(string context, string query, List<ChatMessage> chatHistory)
    {
        var historyText = string.Join("\n", chatHistory
            .Select(x => $"{x.Role}: {x.Content}"));

        var prompt = $@"
You are a financial assistant.

Use conversation history and context to answer.

Conversation:
{historyText}

Context:
{context}

User:
{query}
";

        var request = new
        {
            model = _model,
            prompt = prompt,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync($"{_ollamaEndpoint}/api/generate", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();

        return result?.response ?? "I couldn't generate a response.";
    }
}

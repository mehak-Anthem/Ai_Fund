using System.Text;
using System.Text.Json;

namespace Ai_Fund.Services.Embedding;

public class GeminiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiEmbeddingService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
        _model = configuration["Gemini:Model"] ?? "models/text-embedding-004";
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/{_model}:embedContent?key={_apiKey}";
        
        var payload = new
        {
            content = new { parts = new[] { new { text } } }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);
        
        var values = result.GetProperty("embedding").GetProperty("values").EnumerateArray()
            .Select(x => (float)x.GetDouble()).ToArray();

        return values;
    }
}

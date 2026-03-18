using Ai_Fund.Models;
using System.Text.Json;

namespace Ai_Fund.Services.Embedding;

public class NomicEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaEndpoint;

    public NomicEmbeddingService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _ollamaEndpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var request = new
        {
            model = "nomic-embed-text",
            prompt = text
        };

        var response = await _httpClient.PostAsJsonAsync($"{_ollamaEndpoint}/api/embeddings", request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();
        
        return result?.embedding.ToArray() ?? Array.Empty<float>();
    }
}

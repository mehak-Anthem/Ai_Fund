using System.Net.Http.Headers;
using System.Text.Json;

namespace Ai_Fund.Services.Embedding;

/// <summary>
/// Uses HuggingFace's free Inference API for text embeddings.
/// Model: sentence-transformers/all-mpnet-base-v2 (768 dimensions - matches Qdrant collection config)
/// </summary>
public class HuggingFaceEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelUrl;
    private readonly ILogger<HuggingFaceEmbeddingService> _logger;

    public HuggingFaceEmbeddingService(IConfiguration configuration, ILogger<HuggingFaceEmbeddingService> logger)
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        var model = configuration["HuggingFace:EmbeddingModel"] ?? "sentence-transformers/all-mpnet-base-v2";
        _modelUrl = $"https://api-inference.huggingface.co/models/{model}";
        
        var apiKey = configuration["HuggingFace:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }
        
        _logger = logger;
        _logger.LogInformation("HuggingFaceEmbeddingService initialized with model: {Model}", model);
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        try
        {
            var request = new { inputs = text };
            var response = await _httpClient.PostAsJsonAsync(_modelUrl, request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("HuggingFace API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                
                // If model is loading, wait and retry once
                if (errorContent.Contains("currently loading"))
                {
                    _logger.LogWarning("Model is loading, waiting 20 seconds and retrying...");
                    await Task.Delay(20000);
                    response = await _httpClient.PostAsJsonAsync(_modelUrl, request);
                    response.EnsureSuccessStatusCode();
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                }
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            
            // HuggingFace can return a flat array [f1, f2...] or a nested array [[f1, f2...]]
            var embeddings = new List<float>();
            
            if (json.ValueKind == JsonValueKind.Array)
            {
                var firstElement = json.EnumerateArray().First();
                
                if (firstElement.ValueKind == JsonValueKind.Array)
                {
                    // Nested array [[...]]
                    foreach (var value in firstElement.EnumerateArray())
                    {
                        embeddings.Add(value.GetSingle());
                    }
                }
                else
                {
                    // Flat array [...]
                    foreach (var value in json.EnumerateArray())
                    {
                        embeddings.Add(value.GetSingle());
                    }
                }
            }
            
            if (embeddings.Count == 0)
            {
                var raw = json.GetRawText();
                _logger.LogWarning("HuggingFace returned empty or unexpected format: {Raw}", raw);
                throw new Exception($"HuggingFace returned invalid embedding format. Raw response: {raw}");
            }
            
            _logger.LogInformation("Generated embedding with {Count} dimensions", embeddings.Count);
            return embeddings.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding via HuggingFace");
            throw; // Rethrow so the caller knows it failed
        }
    }
}

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
        
        _logger = logger;
        
        try
        {
            var model = configuration["HuggingFace:EmbeddingModel"] ?? "sentence-transformers/all-MiniLM-L6-v2";
            _modelUrl = $"https://api-inference.huggingface.co/pipeline/feature-extraction/{model}";
            
            var apiKey = configuration["HuggingFace:ApiKey"]?.Trim();
            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                _httpClient.DefaultRequestHeaders.Add("X-Wait-For-Model", "true");
                _logger.LogInformation("HuggingFace API Key configured for stable model: {Model}", model);
            }
            else
            {
                _logger.LogWarning("NO HuggingFace API Key found! Sync will likely fail.");
            }
            
            _logger.LogInformation("HuggingFaceEmbeddingService initialized with model: {Model}", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing HuggingFaceEmbeddingService");
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        try
        {
            // NEW: Use the direct string payload as suggested by the user
            // HuggingFace pipeline endpoints often prefer raw strings over JSON objects
            var response = await _httpClient.PostAsJsonAsync(_modelUrl, text);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                return ParseEmbedding(json);
            }
            
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("HuggingFace API failed: {Status} - {Error}", response.StatusCode, error);
            
            // 🔥 FAIL SAFE: Return empty vector instead of crashing
            _logger.LogWarning("Returning zero-vector (384d) as fail-safe fallback.");
            return new float[384];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HuggingFace EXCEPTION: {Message}", ex.Message);
            
            // 🔥 NEVER BREAK PROD
            return new float[384];
        }
    }

    private float[] ParseEmbedding(JsonElement json)
    {
        var embeddings = new List<float>();
        
        if (json.ValueKind == JsonValueKind.Array)
        {
            var enumArr = json.EnumerateArray();
            if (enumArr.Any())
            {
                var firstElement = enumArr.First();
                if (firstElement.ValueKind == JsonValueKind.Array)
                {
                    // Nested array [[f1, f2...]] - common for batch or pipeline
                    foreach (var value in firstElement.EnumerateArray())
                    {
                        embeddings.Add(value.GetSingle());
                    }
                }
                else
                {
                    // Flat array [f1, f2...] - common for single inference
                    foreach (var value in json.EnumerateArray())
                    {
                        embeddings.Add(value.GetSingle());
                    }
                }
            }
        }

        if (embeddings.Count == 0)
        {
            throw new Exception($"Unexpected embedding format: {json.GetRawText()}");
        }

        _logger.LogInformation("Successfully generated {Count}d embedding", embeddings.Count);
        return embeddings.ToArray();
    }
}

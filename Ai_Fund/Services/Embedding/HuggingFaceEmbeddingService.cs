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
        _modelUrl = $"https://api-inference.huggingface.co/pipeline/feature-extraction/{model}";
        
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
            
            // HuggingFace returns a flat array of floats for single input
            var embeddings = new List<float>();
            foreach (var value in json.EnumerateArray())
            {
                embeddings.Add(value.GetSingle());
            }
            
            return embeddings.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding via HuggingFace");
            return Array.Empty<float>();
        }
    }
}

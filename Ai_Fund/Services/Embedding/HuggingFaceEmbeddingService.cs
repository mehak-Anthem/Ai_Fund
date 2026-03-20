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
            var model = configuration["HuggingFace:EmbeddingModel"] ?? "sentence-transformers/all-mpnet-base-v2";
            _modelUrl = $"https://router.huggingface.co/hf-inference/models/{model}";
            
            var apiKey = configuration["HuggingFace:ApiKey"]?.Trim();
            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                _httpClient.DefaultRequestHeaders.Add("X-Task", "feature-extraction");
                _logger.LogInformation("HuggingFace API Key found and configured (starts with {Start}...)", apiKey.Substring(0, Math.Min(5, apiKey.Length)));
            }
            else
            {
                _logger.LogWarning("NO HuggingFace API Key found in configuration! Check environment variables.");
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
            var request = new 
            { 
                inputs = text,
                options = new { wait_for_model = true }
            };
            int maxRetries = 3;
            int delayMs = 2000;
            HttpResponseMessage? response = null;

            for (int i = 0; i < maxRetries; i++)
            {
                response = await _httpClient.PostAsJsonAsync(_modelUrl, request);
                
                if (response.IsSuccessStatusCode)
                    break;

                var errorContent = await response.Content.ReadAsStringAsync();
                
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests || 
                    errorContent.Contains("currently loading"))
                {
                    _logger.LogWarning("HuggingFace API busy (Retry {Retry}/{Max}): {StatusCode}. Waiting {Delay}ms...", i + 1, maxRetries, response.StatusCode, delayMs);
                    await Task.Delay(delayMs);
                    delayMs *= 2; // Exponential backoff
                }
                else
                {
                    _logger.LogError("HuggingFace API failed: {StatusCode} - {URL} - {Error}", response.StatusCode, _modelUrl, errorContent);
                    response.EnsureSuccessStatusCode();
                }
            }

            response!.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            
            // HuggingFace feature-extraction returns a nested array for single input: [[f1, f2...]]
            // or sometimes a flat array for non-batch: [f1, f2...]
            var embeddings = new List<float>();
            
            if (json.ValueKind == JsonValueKind.Array)
            {
                var enumArr = json.EnumerateArray();
                if (enumArr.Any())
                {
                    var firstElement = enumArr.First();
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
            }
            
            if (embeddings.Count == 0)
            {
                var raw = json.GetRawText();
                _logger.LogWarning("HuggingFace returned empty or unexpected format: {Raw}", raw);
                throw new Exception($"HuggingFace returned invalid embedding format. Raw response: {raw}");
            }
            
            _logger.LogInformation("Generated embedding with {Count} dimensions via feature-extraction pipeline", embeddings.Count);
            return embeddings.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding via HuggingFace");
            throw; // Rethrow so the caller knows it failed
        }
    }
}

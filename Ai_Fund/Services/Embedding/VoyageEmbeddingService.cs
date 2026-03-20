using System.Net.Http.Headers;
using System.Text.Json;
using System.Linq;

namespace Ai_Fund.Services.Embedding;

public class VoyageEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<VoyageEmbeddingService> _logger;

    public VoyageEmbeddingService(IConfiguration configuration, ILogger<VoyageEmbeddingService> logger)
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        _apiKey = configuration["Voyage:ApiKey"]?.Trim() ?? string.Empty;
        _model = configuration["Voyage:Model"] ?? "voyage-3";
        _logger = logger;

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("Voyage API Key is missing! Embeddings will fail.");
        }
        else
        {
            _logger.LogInformation("VoyageEmbeddingService initialized with model: {Model}", _model);
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("Voyage API Key is missing! Returning zero-vector fail-safe.");
                return new float[1024];
            }

            // Drop-in replacement logic as suggested
            var payload = new
            {
                input = text,
                model = _model
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.voyageai.com/v1/embeddings", payload);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Voyage API failed: {Status} - {Error}", response.StatusCode, content);
                return new float[1024]; // Fail-safe
            }

            var result = JsonSerializer.Deserialize<VoyageResponse>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (result?.Data == null || result.Data.Count == 0 || result.Data[0].Embedding == null)
            {
                _logger.LogWarning("Voyage returned empty or malformed data: {Content}", content);
                return new float[1024];
            }

            _logger.LogInformation("Successfully generated {Count}d embedding via Voyage AI", result.Data[0].Embedding.Count());
            return result.Data[0].Embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Voyage AI EXCEPTION: {Message}", ex.Message);
            return new float[1024]; // Fail-safe
        }
    }
}

public class VoyageResponse
{
    public List<VoyageData>? Data { get; set; }
}

public class VoyageData
{
    public float[]? Embedding { get; set; }
}

using System.Net.Http.Headers;
using System.Text.Json;

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
                return new float[1024]; // Fail-safe
            }

            var request = new
            {
                input = new[] { text },
                model = _model
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.voyageai.com/v1/embeddings");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Voyage API failed: {Status} - {Error}", response.StatusCode, content);
                return new float[1024]; // Fail-safe
            }

            using var doc = JsonDocument.Parse(content);
            var embedding = doc.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();

            _logger.LogInformation("Successfully generated {Count}d embedding via Voyage AI", embedding.Length);
            return embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Voyage AI EXCEPTION: {Message}", ex.Message);
            return new float[1024]; // Fail-safe
        }
    }
}

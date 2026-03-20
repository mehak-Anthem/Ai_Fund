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
        _model = configuration["HuggingFace:EmbeddingModel"] ?? configuration["Voyage:Model"] ?? "voyage-2";
        _logger = logger;

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("Voyage API Key is missing! Embeddings will fail.");
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _logger.LogInformation("VoyageEmbeddingService initialized with model: {Model}", _model);
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        try
        {
            var keyHint = string.IsNullOrEmpty(_apiKey) ? "MISSING" : _apiKey.Length > 5 ? _apiKey.Substring(0, 5) + "..." : "TOO_SHORT";
            _logger.LogInformation(">>> VOYAGE START: Model={Model}, KeyHint={KeyHint}, URL={URL}", _model, keyHint, "https://api.voyageai.com/v1/embeddings");

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogError(">>> VOYAGE FAIL: API Key is NULL or EMPTY. Check Render Env Vars.");
                return new float[1024];
            }

            var payload = new
            {
                input = text,
                model = _model
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonPayload = JsonSerializer.Serialize(payload, options);
            _logger.LogInformation(">>> VOYAGE PAYLOAD: {Payload}", jsonPayload);

            var response = await _httpClient.PostAsJsonAsync("https://api.voyageai.com/v1/embeddings", payload, options);
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation(">>> VOYAGE RESPONSE ({Status}): {Content}", response.StatusCode, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(">>> VOYAGE FAIL: Status {Status}. Response: {Content}", response.StatusCode, content);
                return new float[1024]; 
            }

            var result = JsonSerializer.Deserialize<VoyageResponse>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (result?.Data == null || result.Data.Count == 0 || result.Data[0].Embedding == null)
            {
                _logger.LogWarning(">>> VOYAGE FAIL: Malformed JSON or empty data.");
                return new float[1024];
            }

            _logger.LogInformation(">>> VOYAGE SUCCESS: Generated {Count}d embedding", result.Data[0].Embedding.Count());
            return result.Data[0].Embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ">>> VOYAGE EXCEPTION: {Message}", ex.Message);
            return new float[1024];
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

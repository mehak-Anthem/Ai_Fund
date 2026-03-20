using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Ai_Fund.Services;

public class QdrantService : IQdrantService
{
    private readonly QdrantClient _client;
    private readonly string _collectionName;
    private readonly ILogger<QdrantService> _logger;

    public QdrantService(IConfiguration configuration, ILogger<QdrantService> logger)
    {
        var host = configuration["Qdrant:Host"] ?? "localhost";
        var port = int.Parse(configuration["Qdrant:Port"] ?? "6334");
        _collectionName = configuration["Qdrant:CollectionName"] ?? "ai_fund_knowledge";
        var apiKey = configuration["Qdrant:ApiKey"];
        
        if (!string.IsNullOrEmpty(apiKey))
        {
            // Cloud Qdrant: connect via HTTPS with API key
            _client = new QdrantClient(host, https: true, apiKey: apiKey);
            logger.LogInformation("Connected to Qdrant Cloud at {Host}", host);
        }
        else
        {
            // Local Qdrant: connect via gRPC
            _client = new QdrantClient(host, port);
            logger.LogInformation("Connected to local Qdrant at {Host}:{Port}", host, port);
        }
        
        _logger = logger;
    }

    public async Task<bool> CollectionExistsAsync()
    {
        try
        {
            var collections = await _client.ListCollectionsAsync();
            return collections.Any(c => c == _collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if collection exists");
            return false;
        }
    }

    public async Task InitializeCollectionAsync()
    {
        try
        {
            var exists = await CollectionExistsAsync();
            
            if (!exists)
            {
                await _client.CreateCollectionAsync(
                    collectionName: _collectionName,
                    vectorsConfig: new VectorParams
                    {
                        Size = 768, // nomic-embed-text dimension
                        Distance = Distance.Cosine
                    }
                );
                
                _logger.LogInformation("Qdrant collection '{CollectionName}' created successfully", _collectionName);
            }
            else
            {
                _logger.LogInformation("Qdrant collection '{CollectionName}' already exists", _collectionName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Qdrant collection");
            throw;
        }
    }

    public async Task UpsertAsync(int id, float[] vector, string content, Dictionary<string, object>? metadata = null)
    {
        try
        {
            var payload = new Dictionary<string, Value>
            {
                ["content"] = new Value { StringValue = content }
            };

            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    payload[kvp.Key] = new Value { StringValue = kvp.Value?.ToString() ?? "" };
                }
            }

            var point = new PointStruct
            {
                Id = new PointId { Num = (ulong)id },
                Vectors = vector,
                Payload = { payload }
            };

            await _client.UpsertAsync(_collectionName, new[] { point });
            
            _logger.LogDebug("Upserted point {Id} to Qdrant", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting to Qdrant: {Id}", id);
            throw;
        }
    }

    public async Task<List<QdrantSearchResult>> SearchAsync(float[] queryVector, int limit = 3)
    {
        try
        {
            var searchResult = await _client.SearchAsync(
                collectionName: _collectionName,
                vector: queryVector,
                limit: (ulong)limit,
                scoreThreshold: 0.6f
            );

            return searchResult.Select(r => new QdrantSearchResult
            {
                Id = (int)r.Id.Num,
                Score = r.Score,
                Content = r.Payload.ContainsKey("content") ? r.Payload["content"].StringValue : string.Empty,
                Metadata = r.Payload
                    .Where(kvp => kvp.Key != "content")
                    .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value.StringValue)
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Qdrant");
            return new List<QdrantSearchResult>();
        }
    }
}

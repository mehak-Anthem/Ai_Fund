using Ai_Fund.Data.Interfaces;
using Ai_Fund.Services.Embedding;

namespace Ai_Fund.Services;

public interface ISyncService
{
    Task SyncKnowledgeToQdrantAsync();
}

public class SyncService : ISyncService
{
    private readonly IMutualFundRepository _repository;
    private readonly IEmbeddingService _embeddingService;
    private readonly IQdrantService _qdrantService;
    private readonly ILogger<SyncService> _logger;

    public SyncService(
        IMutualFundRepository repository,
        IEmbeddingService embeddingService,
        IQdrantService qdrantService,
        ILogger<SyncService> logger)
    {
        _repository = repository;
        _embeddingService = embeddingService;
        _qdrantService = qdrantService;
        _logger = logger;
    }

    public async Task SyncKnowledgeToQdrantAsync()
    {
        try
        {
            _logger.LogInformation("Starting sync from SQL to Qdrant...");

            // Ensure collection exists
            await _qdrantService.InitializeCollectionAsync();

            // Get all active knowledge from SQL
            var allKnowledge = await _repository.GetAllKnowledgeAsync();

            _logger.LogInformation("Found {Count} active knowledge entries to sync", allKnowledge.Count);

            int syncedCount = 0;
            int skippedCount = 0;

            foreach (var item in allKnowledge)
            {
                try
                {
                    // Skip if content is empty
                    if (string.IsNullOrWhiteSpace(item.Answer))
                    {
                        _logger.LogWarning("Skipping ID {Id}: Empty content", item.Id);
                        skippedCount++;
                        continue;
                    }

                    // Get or generate embedding (MUST be float[768])
                    float[] embedding;
                    
                    if (!string.IsNullOrEmpty(item.Embedding))
                    {
                        // Use existing embedding from SQL
                        embedding = System.Text.Json.JsonSerializer.Deserialize<float[]>(item.Embedding) ?? Array.Empty<float>();
                        
                        // Verify vector size is 768
                        if (embedding.Length != 768)
                        {
                            _logger.LogWarning("ID {Id}: Invalid embedding size {Size}, regenerating...", item.Id, embedding.Length);
                            var normalizedQuestion = TextNormalizer.Normalize(item.Question);
                            embedding = await _embeddingService.GenerateEmbeddingAsync(normalizedQuestion);
                            
                            // Save back to SQL
                            var embeddingJson = System.Text.Json.JsonSerializer.Serialize(embedding);
                            await _repository.UpdateEmbeddingAsync(item.Id, embeddingJson);
                        }
                    }
                    else
                    {
                        // Generate new embedding
                        _logger.LogInformation("Generating embedding for ID {Id}", item.Id);
                        var normalizedQuestion = TextNormalizer.Normalize(item.Question);
                        embedding = await _embeddingService.GenerateEmbeddingAsync(normalizedQuestion);
                        
                        // Save embedding back to SQL
                        var embeddingJson = System.Text.Json.JsonSerializer.Serialize(embedding);
                        await _repository.UpdateEmbeddingAsync(item.Id, embeddingJson);
                    }

                    // Prepare metadata
                    var metadata = new Dictionary<string, object>
                    {
                        ["question"] = item.Question,
                        ["category"] = "MutualFund",
                        ["source"] = "MutualFundKnowledge"
                    };

                    // Upsert to Qdrant
                    await _qdrantService.UpsertAsync(item.Id, embedding, item.Answer, metadata);
                    syncedCount++;

                    _logger.LogDebug("Synced ID {Id}: {Question}", item.Id, item.Question);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing knowledge ID {Id}", item.Id);
                    skippedCount++;
                }
            }

            _logger.LogInformation("✅ Sync completed: {Synced} synced, {Skipped} skipped", syncedCount, skippedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during sync process");
            throw;
        }
    }
}

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
            _logger.LogInformation(">>> MIGRATION: Deleting and recreating collection for 1024-dimension (Voyage AI) change...");

            // Delete existing collection to change dimensions
            await _qdrantService.DeleteCollectionAsync();

            // Ensure collection exists (with new 384 size)
            await _qdrantService.InitializeCollectionAsync();

            // Get all active knowledge from SQL
            var allKnowledge = await _repository.GetAllKnowledgeAsync();

            _logger.LogInformation("Found {Count} active knowledge entries to sync", allKnowledge.Count);

            int total = allKnowledge.Count;
            foreach (var item in allKnowledge)
            {
                try
                {
                    // Skip if content is empty
                    if (string.IsNullOrWhiteSpace(item.Answer))
                    {
                        _logger.LogWarning(">>> SYNC SKIP (ID={Id}): Empty answer.", item.Id);
                        skippedCount++;
                        continue;
                    }

                    _logger.LogInformation(">>> SYNC ITEM ({Current}/{Total}): ID={Id}, Text='{Text}...' ", syncedCount + skippedCount + 1, total, item.Id, item.Question.Substring(0, Math.Min(50, item.Question.Length)));

                    // ALWAYS regenerate embedding for Voyage (1024d)
                    var normalizedQuestion = TextNormalizer.Normalize(item.Question);
                    var embedding = await _embeddingService.GenerateEmbeddingAsync(normalizedQuestion);
                    
                    if (embedding.All(v => v == 0))
                    {
                        _logger.LogWarning(">>> SYNC FAIL (ID={Id}): Embedding service returned zero-vector. Check Voyage connectivity.");
                        skippedCount++;
                        continue;
                    }

                    // Save fresh embedding back to SQL
                    var embeddingJson = System.Text.Json.JsonSerializer.Serialize(embedding);
                    await _repository.UpdateEmbeddingAsync(item.Id, embeddingJson);

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

                    _logger.LogInformation(">>> SYNC SUCCESS (ID={Id})", item.Id);
                    
                    // Add a small delay to avoid hitting Voyage rate limits (bulk)
                    await Task.Delay(200);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ">>> SYNC EXCEPTION (ID={Id}): {Message}", item.Id, ex.Message);
                    skippedCount++;
                }
            }

            _logger.LogInformation(">>> SYNC FINISHED: {Synced} synced, {Skipped} skipped to Qdrant Cloud (1024d).", syncedCount, skippedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during sync process");
            throw;
        }
    }
}

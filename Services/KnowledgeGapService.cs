using Ai_Fund.Data.Interfaces;
using Ai_Fund.Models;

namespace Ai_Fund.Services;

public class KnowledgeGapService : IKnowledgeGapService
{
    private readonly IMutualFundRepository _repository;
    private readonly ILogger<KnowledgeGapService> _logger;

    public KnowledgeGapService(IMutualFundRepository repository, ILogger<KnowledgeGapService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task LogGapAsync(string question, string intent, double confidence)
    {
        try
        {
            var existing = await _repository.GetKnowledgeGapByQuestionAsync(question);

            if (existing != null)
            {
                existing.OccurrenceCount++;
                existing.LastAsked = DateTime.UtcNow;
                await _repository.UpdateKnowledgeGapAsync(existing);
                
                _logger.LogInformation("Knowledge gap updated: {Question}, Count: {Count}", question, existing.OccurrenceCount);
            }
            else
            {
                var newGap = new KnowledgeGap
                {
                    Question = question,
                    DetectedIntent = intent,
                    ConfidenceScore = confidence,
                    OccurrenceCount = 1,
                    LastAsked = DateTime.UtcNow,
                    Status = "New",
                    CreatedAt = DateTime.UtcNow
                };

                await _repository.SaveKnowledgeGapAsync(newGap);
                
                _logger.LogWarning("New knowledge gap detected: {Question}, Confidence: {Confidence}", question, confidence);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging knowledge gap: {Question}", question);
        }
    }

    public async Task<List<KnowledgeGap>> GetTopGapsAsync(int count = 10)
    {
        return await _repository.GetTopKnowledgeGapsAsync(count);
    }

    public async Task ResolveGapAsync(int gapId, string answer)
    {
        var gap = await _repository.GetKnowledgeGapByQuestionAsync("");
        // Implementation for resolving gap
        // This would update the gap status and add to knowledge base
    }
}

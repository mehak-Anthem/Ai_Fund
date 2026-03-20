using Microsoft.AspNetCore.Mvc;
using Ai_Fund.Services;
using Ai_Fund.Data.Interfaces;
using Ai_Fund.Services.Embedding;
using System.Text.Json;

namespace Ai_Fund.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgeGapController : ControllerBase
{
    private readonly IKnowledgeGapService _gapService;
    private readonly IMutualFundRepository _repository;
    private readonly IEmbeddingService _embeddingService;

    public KnowledgeGapController(
        IKnowledgeGapService gapService,
        IMutualFundRepository repository,
        IEmbeddingService embeddingService)
    {
        _gapService = gapService;
        _repository = repository;
        _embeddingService = embeddingService;
    }

    [HttpGet("top-gaps")]
    public async Task<IActionResult> GetTopGaps([FromQuery] int count = 10)
    {
        var gaps = await _gapService.GetTopGapsAsync(count);
        return Ok(gaps);
    }

    [HttpPost("resolve/{gapId}")]
    public async Task<IActionResult> ResolveGap(int gapId, [FromBody] ResolveGapRequest request)
    {
        // Add to knowledge base
        await _repository.AddKnowledgeFromGapAsync(request.Question, request.Answer);

        // Generate embedding for the new knowledge
        var normalizedQuestion = TextNormalizer.Normalize(request.Question);
        var embedding = await _embeddingService.GenerateEmbeddingAsync(normalizedQuestion);
        var embeddingJson = JsonSerializer.Serialize(embedding);

        // Get the newly added knowledge ID (simplified - in production, return ID from insert)
        var allKnowledge = await _repository.GetAllKnowledgeAsync();
        var newKnowledge = allKnowledge.OrderByDescending(x => x.Id).FirstOrDefault();
        
        if (newKnowledge.Id > 0)
        {
            await _repository.UpdateEmbeddingAsync(newKnowledge.Id, embeddingJson);
        }

        // Update gap status
        var gap = await _repository.GetKnowledgeGapByQuestionAsync(request.Question);
        if (gap != null)
        {
            gap.Status = "Resolved";
            gap.SuggestedAnswer = request.Answer;
            await _repository.UpdateKnowledgeGapAsync(gap);
        }

        return Ok(new { Message = $"Knowledge gap {gapId} resolved and added to knowledge base" });
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var topGaps = await _gapService.GetTopGapsAsync(10);
        
        return Ok(new
        {
            TotalGaps = topGaps.Count,
            TopMissingQuestions = topGaps.Select(g => new
            {
                g.Question,
                g.OccurrenceCount,
                g.LastAsked,
                g.Status,
                g.ConfidenceScore
            }).ToList(),
            Summary = new
            {
                NewGaps = topGaps.Count(g => g.Status == "New"),
                ReviewingGaps = topGaps.Count(g => g.Status == "Reviewing"),
                ResolvedGaps = topGaps.Count(g => g.Status == "Resolved")
            }
        });
    }

    [HttpPost("sync-to-qdrant")]
    public async Task<IActionResult> SyncToQdrant([FromServices] ISyncService syncService)
    {
        try
        {
            await syncService.SyncKnowledgeToQdrantAsync();
            return Ok(new { Message = "Knowledge synced to Qdrant successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpGet("qdrant-status")]
    public async Task<IActionResult> GetQdrantStatus([FromServices] IQdrantService qdrantService)
    {
        var exists = await qdrantService.CollectionExistsAsync();
        return Ok(new
        {
            CollectionExists = exists,
            CollectionName = "ai_fund_knowledge",
            Status = exists ? "Ready" : "Not Initialized"
        });
    }

    [HttpGet("test-search")]
    public async Task<IActionResult> TestSearch(
        [FromServices] IQdrantService qdrantService,
        [FromQuery] string query = "SIP")
    {
        try
        {
            // Normalize and generate embedding
            var normalizedQuery = TextNormalizer.Normalize(query);
            var embedding = await _embeddingService.GenerateEmbeddingAsync(normalizedQuery);

            // Search Qdrant with very low threshold for debugging
            var results = await qdrantService.SearchAsync(embedding, limit: 5);
            
            return Ok(new
            {
                Query = query,
                NormalizedQuery = normalizedQuery,
                ResultsCount = results.Count,
                Results = results.Select(r => new
                {
                    r.Id,
                    r.Score,
                    r.Content,
                    Question = r.Metadata?.ContainsKey("question") == true ? r.Metadata["question"] : null
                })
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }
}

public class ResolveGapRequest
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

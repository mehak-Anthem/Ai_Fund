using Microsoft.AspNetCore.Mvc;
using Ai_Fund.Data.Interfaces;
using Ai_Fund.Models;
using Ai_Fund.Services.Embedding;
using Ai_Fund.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Ai_Fund.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{

    private readonly IMutualFundRepository _repository;
    private readonly IEmbeddingService _embeddingService;
    private readonly IAuthService _authService;

    public AdminController(
        IMutualFundRepository repository,
        IEmbeddingService embeddingService,
        IAuthService authService)
    {
        _repository = repository;
        _embeddingService = embeddingService;
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (response == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        if (!string.Equals(response.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        return Ok(new
        {
            token = response.Token,
            user = new
            {
                id = response.UserId,
                username = response.Username,
                role = response.Role
            }
        });
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var totalQueries = await _repository.GetAiLogCountAsync();
        var avgConfidence = await _repository.GetAverageConfidenceAsync();
        var activeUsers = await _repository.GetActiveUserCountAsync();
        var unanswered = await _repository.GetUnansweredKnowledgeGapCountAsync();

        var aiPerformanceScore = Math.Round(((avgConfidence * 0.7) + (Math.Max(0, 1 - (unanswered / 100.0)) * 0.3)) * 100, 1);

        return Ok(new AdminDashboardStatsResponse
        {
            TotalQueries = totalQueries,
            AvgConfidence = Math.Round(avgConfidence, 4),
            Unanswered = unanswered,
            ActiveUsers = activeUsers,
            AiPerformanceScore = aiPerformanceScore
        });
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        var queryCounts = await _repository.GetDailyQueryCountsAsync();
        var confidenceTrend = await _repository.GetDailyConfidenceTrendAsync();
        var categories = await _repository.GetIntentCategoryUsageAsync();

        return Ok(new AdminAnalyticsResponse
        {
            QueriesOverTime = queryCounts.Select(item => new AdminTimeSeriesItem
            {
                Date = item.Date.ToString("yyyy-MM-dd"),
                Value = item.Count
            }).ToList(),
            ConfidenceTrend = confidenceTrend.Select(item => new AdminTimeSeriesItem
            {
                Date = item.Date.ToString("yyyy-MM-dd"),
                Value = Math.Round(item.Value * 100, 2)
            }).ToList(),
            CategoryUsage = categories.Select(item => new AdminCategoryUsageItem
            {
                Category = item.Category,
                Count = item.Count
            }).ToList()
        });
    }

    [HttpGet("knowledge-gaps")]
    public async Task<IActionResult> GetKnowledgeGaps([FromQuery] bool includeResolved = true, [FromQuery] int top = 100)
    {
        var gaps = await _repository.GetKnowledgeGapsAsync(includeResolved, top);
        return Ok(gaps.Select(MapKnowledgeGap));
    }

    [HttpPut("knowledge-gap/{id}")]
    public async Task<IActionResult> UpdateKnowledgeGapStatus(int id, [FromBody] AdminKnowledgeGapUpdateRequest request)
    {
        var gap = await _repository.GetKnowledgeGapByIdAsync(id);
        if (gap == null)
        {
            return NotFound(new { message = "Knowledge gap not found" });
        }

        gap.Status = request.Status;
        gap.LastAsked = DateTime.UtcNow;
        await _repository.UpdateKnowledgeGapAsync(gap);

        return Ok(MapKnowledgeGap(gap));
    }

    [HttpGet("trending-queries")]
    public async Task<IActionResult> GetTrendingQueries([FromQuery] int top = 10)
    {
        var queries = await _repository.GetTrendingQueriesAsync(top);
        return Ok(queries.Select(item => new
        {
            query = item.Query,
            count = item.Count,
            avgConfidence = Math.Round(item.AvgConfidence, 4)
        }));
    }

    [HttpPost("generate-embeddings")]
    public async Task<IActionResult> GenerateEmbeddings()
    {
        var allData = await _repository.GetAllKnowledgeAsync();
        int count = 0;

        foreach (var item in allData)
        {
            if (string.IsNullOrEmpty(item.Embedding))
            {
                // Normalize question before generating embedding
                var normalizedQuestion = Services.TextNormalizer.Normalize(item.Question);
                var embedding = await _embeddingService.GenerateEmbeddingAsync(normalizedQuestion);
                var embeddingJson = JsonSerializer.Serialize(embedding);
                await _repository.UpdateEmbeddingAsync(item.Id, embeddingJson);
                count++;
            }
        }

        return Ok($"Generated embeddings for {count} records");
    }

    [HttpPost("regenerate-embeddings")]
    public async Task<IActionResult> RegenerateEmbeddings()
    {
        var allData = await _repository.GetAllKnowledgeAsync();
        int count = 0;

        foreach (var item in allData)
        {
            // Normalize question before generating embedding
            var normalizedQuestion = Services.TextNormalizer.Normalize(item.Question);
            var embedding = await _embeddingService.GenerateEmbeddingAsync(normalizedQuestion);
            var embeddingJson = JsonSerializer.Serialize(embedding);
            await _repository.UpdateEmbeddingAsync(item.Id, embeddingJson);
            count++;
        }

        return Ok($"Regenerated embeddings for {count} records with normalization");
    }

    [HttpGet("check-embeddings")]
    public async Task<IActionResult> CheckEmbeddings()
    {
        var allData = await _repository.GetAllKnowledgeAsync();
        var result = allData.Select(x => new
        {
            x.Id,
            x.Question,
            HasEmbedding = !string.IsNullOrEmpty(x.Embedding),
            EmbeddingLength = x.Embedding?.Length ?? 0
        });

        return Ok(result);
    }

    [HttpPost("deactivate-knowledge/{id}")]
    public async Task<IActionResult> DeactivateKnowledge(int id)
    {
        await _repository.DeactivateKnowledgeAsync(id);
        return Ok($"Knowledge entry {id} deactivated");
    }

    [HttpPost("activate-knowledge/{id}")]
    public async Task<IActionResult> ActivateKnowledge(int id)
    {
        await _repository.ActivateKnowledgeAsync(id);
        return Ok($"Knowledge entry {id} activated");
    }

    [HttpPost("update-version/{id}")]
    public async Task<IActionResult> UpdateVersion(int id, [FromQuery] int version)
    {
        await _repository.UpdateKnowledgeVersionAsync(id, version);
        return Ok($"Knowledge entry {id} updated to version {version}");
    }

    private static AdminKnowledgeGapResponse MapKnowledgeGap(KnowledgeGap gap) => new()
    {
        Id = gap.Id.ToString(),
        Question = gap.Question,
        ConfidenceScore = gap.ConfidenceScore,
        Status = gap.Status,
        Count = gap.OccurrenceCount,
        LastAsked = gap.LastAsked,
        CreatedAt = gap.CreatedAt
    };
}

public class AdminDashboardStatsResponse
{
    public int TotalQueries { get; set; }
    public double AvgConfidence { get; set; }
    public int Unanswered { get; set; }
    public int ActiveUsers { get; set; }
    public double AiPerformanceScore { get; set; }
}

public class AdminAnalyticsResponse
{
    public List<AdminTimeSeriesItem> QueriesOverTime { get; set; } = [];
    public List<AdminTimeSeriesItem> ConfidenceTrend { get; set; } = [];
    public List<AdminCategoryUsageItem> CategoryUsage { get; set; } = [];
}

public class AdminTimeSeriesItem
{
    public string Date { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class AdminCategoryUsageItem
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AdminKnowledgeGapResponse
{
    public string Id { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime LastAsked { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminKnowledgeGapUpdateRequest
{
    public string Status { get; set; } = "New";
}

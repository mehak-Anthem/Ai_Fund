using Microsoft.AspNetCore.Mvc;
using Ai_Fund.Data.Interfaces;
using Ai_Fund.Services.Embedding;
using System.Text.Json;

using Microsoft.AspNetCore.Authorization;

namespace Ai_Fund.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{

    private readonly IMutualFundRepository _repository;
    private readonly IEmbeddingService _embeddingService;

    public AdminController(IMutualFundRepository repository, IEmbeddingService embeddingService)
    {
        _repository = repository;
        _embeddingService = embeddingService;
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

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics([FromQuery] string? userId = null)
    {
        await Task.CompletedTask;

        // This is a simple implementation - in production, create a proper analytics repository
        return Ok(new
        {
            Message = "Analytics endpoint - implement with AiLog queries",
            Suggestion = "Query AiLog table for most asked questions, failed queries, low confidence answers"
        });
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
}

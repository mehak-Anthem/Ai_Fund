using Microsoft.AspNetCore.Mvc;
using Ai_Fund.Data.Interfaces;
using Ai_Fund.Services.Embedding;
using System.Text.Json;

namespace Ai_Fund.Controllers;

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
                var embedding = await _embeddingService.GenerateEmbeddingAsync(item.Question);
                var embeddingJson = JsonSerializer.Serialize(embedding);
                await _repository.UpdateEmbeddingAsync(item.Id, embeddingJson);
                count++;
            }
        }

        return Ok($"Generated embeddings for {count} records");
    }
}

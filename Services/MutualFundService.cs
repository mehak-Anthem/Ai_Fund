using Ai_Fund.Data.Interfaces;
using Ai_Fund.Services.Embedding;
using Ai_Fund.Models;
using System.Text.Json;

namespace Ai_Fund.Services;

public class MutualFundService : IMutualFundService
{
    private readonly IMutualFundRepository _repository;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILLMService _llmService;
    private static List<ChatMessage> _chatHistory = new List<ChatMessage>();

    public MutualFundService(IMutualFundRepository repository, IEmbeddingService embeddingService, ILLMService llmService)
    {
        _repository = repository;
        _embeddingService = embeddingService;
        _llmService = llmService;
    }

    public async Task<string> GetAnswerAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return "Please provide a valid query";

        // Generate embedding for the query
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);
        
        // Get all knowledge from database
        var allData = await _repository.GetAllKnowledgeAsync();

        // Find best match using cosine similarity
        var bestMatch = allData
            .Where(x => !string.IsNullOrEmpty(x.Embedding))
            .Select(x => new
            {
                Data = x,
                Score = VectorHelper.CosineSimilarity(
                    queryEmbedding,
                    JsonSerializer.Deserialize<float[]>(x.Embedding) ?? Array.Empty<float>()
                )
            })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        if (bestMatch == null || bestMatch.Score < 0.7)
            return "I don't have enough information.";

        return bestMatch.Data.Answer;
    }

    public async Task<string> GetAIAnswerAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return "Please provide a valid query";

        // Save user message
        _chatHistory.Add(new ChatMessage { Role = "User", Content = query });

        // Generate embedding for the query
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);
        
        // Get all knowledge from database
        var allData = await _repository.GetAllKnowledgeAsync();

        // Find top 3 matches using cosine similarity
        var topMatches = allData
            .Where(x => !string.IsNullOrEmpty(x.Embedding))
            .Select(x => new
            {
                Data = x,
                Score = VectorHelper.CosineSimilarity(
                    queryEmbedding,
                    JsonSerializer.Deserialize<float[]>(x.Embedding) ?? Array.Empty<float>()
                )
            })
            .OrderByDescending(x => x.Score)
            .Take(3)
            .ToList();

        if (!topMatches.Any() || topMatches[0].Score < 0.6)
        {
            var noInfoResponse = "I don't have enough information.";
            _chatHistory.Add(new ChatMessage { Role = "Assistant", Content = noInfoResponse });
            
            // Keep only last 5 messages
            if (_chatHistory.Count > 5)
                _chatHistory.RemoveAt(0);
            
            return noInfoResponse;
        }

        // Combine top 3 contexts
        var context = string.Join("\n", topMatches.Select(x => x.Data.Answer));

        // Generate AI response using LLM with chat history
        var aiResponse = await _llmService.AskLLMAsync(context, query, _chatHistory);

        // Safety filter
        if (aiResponse.Contains("guarantee", StringComparison.OrdinalIgnoreCase) || 
            aiResponse.Contains("$") ||
            aiResponse.Contains("₹"))
        {
            return "I can provide general information but not financial advice.";
        }

        // Save bot response
        _chatHistory.Add(new ChatMessage { Role = "Assistant", Content = aiResponse });

        // Keep only last 5 messages
        if (_chatHistory.Count > 5)
            _chatHistory.RemoveAt(0);

        return aiResponse;
    }
}

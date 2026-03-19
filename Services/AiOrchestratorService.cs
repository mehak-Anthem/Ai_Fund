using Ai_Fund.Data.Interfaces;
using Ai_Fund.Models;
using Ai_Fund.Services.Embedding;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Ai_Fund.Services;

public class AiOrchestratorService : IAiOrchestratorService
{
    private readonly IMutualFundRepository _repository;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILLMService _llmService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AiOrchestratorService> _logger;
    private readonly IKnowledgeGapService _gapService;
    private readonly IContextManager _contextManager;

    public AiOrchestratorService(
        IMutualFundRepository repository,
        IEmbeddingService embeddingService,
        ILLMService llmService,
        IMemoryCache cache,
        ILogger<AiOrchestratorService> logger,
        IKnowledgeGapService gapService,
        IContextManager contextManager)
    {
        _repository = repository;
        _embeddingService = embeddingService;
        _llmService = llmService;
        _cache = cache;
        _logger = logger;
        _gapService = gapService;
        _contextManager = contextManager;
    }

    public async Task<ChatResponse> ProcessQueryAsync(string query, string userId)
    {
        try
        {
            // 1. Validate input
            if (string.IsNullOrWhiteSpace(query))
            {
                return CreateResponse("Please provide a valid query", "System", 0, "INVALID");
            }

            // 2. Normalize input (fix typos)
            query = InputNormalizer.NormalizeInput(query);
            _logger.LogInformation("Normalized query: {Query}", query);

            // 3. Resolve follow-up queries with context
            var originalQuery = query;
            query = _contextManager.ResolveFollowUp(query, userId);
            
            if (query != originalQuery)
            {
                _logger.LogInformation("Resolved follow-up: {Original} -> {Resolved}", originalQuery, query);
            }

            // 4. Check for opinion queries
            if (QueryNormalizer.IsOpinionQuery(query))
            {
                return CreateResponse("I can provide general information, but I cannot give personal opinions or financial advice.", "Static", 1.0, "OPINION");
            }

            // 5. Normalize query (remove opinion phrases)
            query = QueryNormalizer.NormalizeQuery(query);

            // 6. Detect intent
            var intent = IntentDetector.DetectIntent(query);
            _logger.LogInformation("Query: {Query}, Intent: {Intent}, UserId: {UserId}", query, intent, userId);

            // 3. Handle static intents
            var staticResponse = HandleStaticIntent(intent);
            if (staticResponse != null)
                return staticResponse;

            // 4. Extract topic from query for context tracking
            var topic = ExtractTopic(query);

            // 4. Check cache
            var cacheKey = $"query_{query.ToLower().Trim()}";
            if (_cache.TryGetValue<ChatResponse>(cacheKey, out var cachedResponse))
            {
                _logger.LogInformation("Cache hit for query: {Query}", query);
                return cachedResponse;
            }

            // 5. Normalize query
            var normalizedQuery = TextNormalizer.Normalize(query);

            // 6. Generate embedding
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(normalizedQuery);

            // 7. Retrieve data (hybrid search)
            var allData = await _repository.GetAllKnowledgeAsync();
            var topMatches = RetrieveTopMatches(allData, queryEmbedding, normalizedQuery);

            // 8. Check if data found
            if (!topMatches.Any())
            {
                _logger.LogWarning("No matches found for query: {Query}", query);
                
                // Log knowledge gap - no vector results
                await _gapService.LogGapAsync(query, intent, 0);
                
                return CreateResponse("I'm not sure I understand. Could you please clarify or rephrase your question? For example, you can ask about SIP, mutual funds, or investments.", "System", 0, intent);
            }

            // Check for low confidence and log gap
            if (topMatches[0].Score < 0.6)
            {
                // Only log if not a useless query
                if (!InputNormalizer.IsUselessQuery(query, topMatches[0].Score))
                {
                    await _gapService.LogGapAsync(query, intent, topMatches[0].Score);
                }
                
                // If very low confidence, ask for clarification
                if (topMatches[0].Score < 0.3)
                {
                    return CreateResponse("I'm not quite sure what you're asking about. Could you provide more details? For example, are you asking about SIP, mutual funds, NAV, or something else?", "System", topMatches[0].Score, intent);
                }
            }

            // Store context for follow-up questions
            if (!string.IsNullOrEmpty(topic))
            {
                _contextManager.SetLastTopic(userId, topic, intent);
            }

            // 9. Build context
            var context = BuildContext(topMatches);

            // 10. For high confidence matches, return direct answer (no LLM rewriting)
            if (topMatches[0].Score > 0.8)
            {
                var directAnswer = topMatches[0].Data.Answer;
                
                // Add contextual prefix
                var prefix = ResponseEnhancer.GetContextualPrefix(query);
                var finalAnswer = prefix + directAnswer;
                
                // Save to chat history
                await _repository.SaveChatHistoryAsync(new Models.ChatHistory
                {
                    UserId = userId,
                    Role = "Assistant",
                    Message = finalAnswer,
                    CreatedDate = DateTime.UtcNow
                });

                // Save AI log
                await _repository.SaveAiLogAsync(new Models.AiLog
                {
                    UserId = userId,
                    Query = query,
                    Response = finalAnswer,
                    ConfidenceScore = topMatches[0].Score,
                    Intent = intent,
                    Source = "Database",
                    CreatedDate = DateTime.UtcNow
                });

                var dbResponse = CreateResponse(finalAnswer, "Database", topMatches[0].Score, intent);
                
                // Cache the response
                var dbCacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };
                CacheExtensions.Set(_cache, cacheKey, dbResponse, dbCacheOptions);
                
                return dbResponse;
            }

            // 11. For simple definitions with high score, return direct answer
            if (intent == "DEFINITION" && topMatches[0].Score > 0.8)
            {
                var directAnswer = topMatches[0].Data.Answer;
                var response = CreateResponse(directAnswer, "Database", topMatches[0].Score, intent);
                
                // Cache the response
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };
                CacheExtensions.Set(_cache, cacheKey, response, cacheOptions);
                
                return response;
            }

            // 11. Call LLM
            var chatHistoryRecords = await _repository.GetChatHistoryAsync(userId);
            var chatHistory = chatHistoryRecords.Select(x => new ChatMessage
            {
                Role = x.Role,
                Content = x.Message
            }).ToList();
            
            // Save user message to DB
            await _repository.SaveChatHistoryAsync(new Models.ChatHistory
            {
                UserId = userId,
                Role = "User",
                Message = query,
                CreatedDate = DateTime.UtcNow
            });
            
            var aiResponse = await _llmService.AskLLMAsync(context, query, chatHistory);

            // Add contextual prefix for natural conversation
            var responsePrefix = ResponseEnhancer.GetContextualPrefix(query);
            aiResponse = responsePrefix + aiResponse;

            // Clean response
            aiResponse = CleanResponse(aiResponse);

            // 12. Apply guardrails
            var guardedResponse = ApplyGuardrails(aiResponse);
            if (guardedResponse != null)
            {
                _logger.LogWarning("Guardrail triggered for query: {Query}", query);
                return guardedResponse;
            }

            // 13. Create final response
            var finalResponse = CreateResponse(aiResponse, "RAG+LLM", topMatches[0].Score, intent);

            // Save AI response to DB
            await _repository.SaveChatHistoryAsync(new Models.ChatHistory
            {
                UserId = userId,
                Role = "Assistant",
                Message = aiResponse,
                CreatedDate = DateTime.UtcNow
            });

            // Save AI log for monitoring
            await _repository.SaveAiLogAsync(new Models.AiLog
            {
                UserId = userId,
                Query = query,
                Response = aiResponse,
                ConfidenceScore = topMatches[0].Score,
                Intent = intent,
                Source = "RAG+LLM",
                CreatedDate = DateTime.UtcNow
            });

            // 14. Cache the response
            var finalCacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };
            CacheExtensions.Set(_cache, cacheKey, finalResponse, finalCacheOptions);

            // 15. Log success
            _logger.LogInformation("Successfully processed query: {Query}, Confidence: {Confidence}", query, finalResponse.Confidence);

            return finalResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query: {Query}", query);
            return CreateResponse("An error occurred while processing your request.", "Error", 0, "ERROR");
        }
    }

    private ChatResponse? HandleStaticIntent(string intent)
    {
        return intent switch
        {
            "GREETING" => CreateResponse("Hello! I can help you with mutual fund queries.", "Static", 1.0, intent),
            "CLOSING" => CreateResponse("Thank you for using our service. Have a great day!", "Static", 1.0, intent),
            "ADVICE" => CreateResponse("I can provide general information, but I cannot give personalized financial advice.", "Static", 1.0, intent),
            _ => null
        };
    }

    private List<(KnowledgeData Data, double Score)> RetrieveTopMatches(
        List<(int Id, string Question, string Answer, string Embedding)> allData,
        float[] queryEmbedding,
        string normalizedQuery)
    {
        return allData
            .Where(x => !string.IsNullOrEmpty(x.Embedding))
            .Select(x =>
            {
                try
                {
                    var embedding = JsonSerializer.Deserialize<float[]>(x.Embedding);
                    if (embedding == null || embedding.Length == 0)
                        return (Data: (KnowledgeData?)null, Score: 0.0);

                    var score = VectorHelper.CosineSimilarity(queryEmbedding, embedding);
                    
                    // Hybrid search: boost if keyword match
                    if (x.Question.ToLower().Contains(normalizedQuery))
                        score += 0.1;

                    // Ensure score doesn't exceed 1.0
                    score = Math.Min(score, 1.0);

                    return (Data: (KnowledgeData?)new KnowledgeData { Id = x.Id, Question = x.Question, Answer = x.Answer, Embedding = x.Embedding }, Score: score);
                }
                catch
                {
                    return (Data: (KnowledgeData?)null, Score: 0.0);
                }
            })
            .Where(x => x.Data != null && x.Score > 0.65)
            .OrderByDescending(x => x.Score)
            .Take(3)
            .ToList();
    }

    private string BuildContext(List<(KnowledgeData Data, double Score)> topMatches)
    {
        return string.Join("\n",
            topMatches
                .Select(x => x.Data.Answer)
                .Distinct());
    }

    private ChatResponse? ApplyGuardrails(string aiResponse)
    {
        // Multi-layer safety checks
        if (aiResponse.Contains("guarantee", StringComparison.OrdinalIgnoreCase) ||
            aiResponse.Contains("fixed return", StringComparison.OrdinalIgnoreCase))
        {
            return CreateResponse("Mutual funds are subject to market risk. No returns are guaranteed.", "SafetyFilter", 0, "BLOCKED");
        }

        if (aiResponse.Contains("$") || aiResponse.Contains("₹"))
        {
            return CreateResponse("I can provide general information, but not financial advice.", "SafetyFilter", 0, "BLOCKED");
        }

        return null;
    }

    private string CleanResponse(string response)
    {
        // Remove common LLM artifacts
        var cleanPatterns = new[]
        {
            "SIPI", "sipi", "SIPIndications", "SIPV", "sipv",
            "languaire", "slapg", "Single-Issue Plan",
            "Systematic Invested Plan Investment Vehicle"
        };
        
        foreach (var pattern in cleanPatterns)
        {
            response = response.Replace(pattern, "SIP", StringComparison.OrdinalIgnoreCase);
        }
        
        // Remove prompt artifacts and guidelines
        var artifactKeywords = new[] 
        { 
            "STRICT RULE", "Guidelines:", "Task:", "Your Role:",
            "professional financial content writer", "Keep the exact same information",
            "Maintaine all", "Do not use slaing"
        };
        
        foreach (var keyword in artifactKeywords)
        {
            if (response.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                // If response contains artifacts, try to extract clean answer
                var sentences = response.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                var cleanSentences = sentences
                    .Where(s => !artifactKeywords.Any(k => s.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    .Where(s => s.Length > 20) // Keep only substantial sentences
                    .Take(3);
                
                if (cleanSentences.Any())
                {
                    response = string.Join(". ", cleanSentences).Trim() + ".";
                }
            }
        }
        
        // Remove unwanted greetings
        if (response.StartsWith("Greetings!", StringComparison.OrdinalIgnoreCase) ||
            response.StartsWith("Hello!", StringComparison.OrdinalIgnoreCase) ||
            response.StartsWith("As a professional", StringComparison.OrdinalIgnoreCase))
        {
            var sentences = response.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            if (sentences.Length > 1)
            {
                response = string.Join(". ", sentences.Skip(1)).Trim();
            }
        }
        
        // Remove unwanted advice phrases
        if (response.Contains("banker", StringComparison.OrdinalIgnoreCase) || 
            response.Contains("advisor", StringComparison.OrdinalIgnoreCase))
        {
            var sentences = response.Split('.');
            var filtered = sentences.Where(s => 
                !s.Contains("banker", StringComparison.OrdinalIgnoreCase) && 
                !s.Contains("advisor", StringComparison.OrdinalIgnoreCase)
            );
            response = string.Join(".", filtered).Trim();
        }
        
        return response.Trim();
    }

    private string ExtractTopic(string query)
    {
        // Extract main topic from query (simple keyword extraction)
        var keywords = new[] { "SIP", "mutual fund", "investment", "NAV", "return", "risk" };
        
        foreach (var keyword in keywords)
        {
            if (query.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return keyword;
            }
        }
        
        // Return first meaningful word if no keyword found
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length > 0 ? words[0] : string.Empty;
    }

    private ChatResponse CreateResponse(string answer, string source, double confidence, string intent)
    {
        return new ChatResponse
        {
            Answer = answer,
            Source = source,
            Confidence = confidence,
            Intent = intent
        };
    }
}

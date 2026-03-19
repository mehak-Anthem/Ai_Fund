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
    private readonly IRewriteService _rewriteService;
    private readonly IPersonalityService _personalityService;
    private readonly IExpansionService _expansionService;
    private readonly IComparisonService _comparisonService;
    private readonly IStructuredAnswerService _structuredAnswerService;
    private readonly ISmartGuidanceService _smartGuidanceService;

    public AiOrchestratorService(
        IMutualFundRepository repository,
        IEmbeddingService embeddingService,
        ILLMService llmService,
        IMemoryCache cache,
        ILogger<AiOrchestratorService> logger,
        IKnowledgeGapService gapService,
        IContextManager contextManager,
        IRewriteService rewriteService,
        IPersonalityService personalityService,
        IExpansionService expansionService,
        IComparisonService comparisonService,
        IStructuredAnswerService structuredAnswerService,
        ISmartGuidanceService smartGuidanceService)
    {
        _repository = repository;
        _embeddingService = embeddingService;
        _llmService = llmService;
        _cache = cache;
        _logger = logger;
        _gapService = gapService;
        _contextManager = contextManager;
        _rewriteService = rewriteService;
        _personalityService = personalityService;
        _expansionService = expansionService;
        _comparisonService = comparisonService;
        _structuredAnswerService = structuredAnswerService;
        _smartGuidanceService = smartGuidanceService;
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

            // 2.5. Handle "how are you" greeting
            var lowerQuery = query.ToLower();
            if (lowerQuery.Contains("how are you") || lowerQuery.Contains("how are u") || 
                lowerQuery.Contains("how r you") || lowerQuery.Contains("how r u"))
            {
                return CreateResponse("I'm doing great 😊 How can I help you with mutual funds today?", "Static", 1.0, "GREETING");
            }

            // 3. Resolve follow-up queries with context
            var originalQuery = query;
            var isExpansion = _expansionService.IsExpansionQuery(query);
            var isComparison = _comparisonService.IsComparisonQuery(query);
            
            // Handle comparison mode
            if (isComparison)
            {
                // Try to extract all entities from current query
                var allEntities = _comparisonService.ExtractAllEntities(query);
                
                if (allEntities.Count >= 2)
                {
                    // Save first two entities for context
                    _contextManager.SaveLastEntities(userId, allEntities[0], allEntities[1]);
                    
                    // Build multi-entity comparison query
                    if (allEntities.Count == 2)
                    {
                        query = $"Compare {allEntities[0]} and {allEntities[1]} - explain differences, benefits, and risks";
                    }
                    else if (allEntities.Count >= 3)
                    {
                        var entityList = string.Join(", ", allEntities.Take(allEntities.Count - 1)) + " and " + allEntities.Last();
                        query = $"Compare {entityList} - explain key differences, benefits, and risks of each";
                    }
                    
                    _logger.LogInformation("Multi-comparison: {Count} entities - {Query}", allEntities.Count, query);
                }
                else
                {
                    // Try to resolve using context (e.g., "difference between both")
                    query = _comparisonService.ResolveComparison(query, userId, _contextManager);
                    _logger.LogInformation("Comparison resolved: {Original} -> {Resolved}", originalQuery, query);
                }
            }
            // Handle expansion mode
            else if (isExpansion)
            {
                var lastTopic = _contextManager.GetLastTopic(userId);
                if (!string.IsNullOrEmpty(lastTopic))
                {
                    query = _expansionService.ExpandQuery(query, lastTopic);
                    _logger.LogInformation("Expansion mode: {Original} -> {Expanded}", originalQuery, query);
                }
            }
            else
            {
                query = _contextManager.ResolveFollowUp(query, userId);
            }
            
            if (query != originalQuery && !isExpansion && !isComparison)
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

            // 6.5. Check for personal/guidance queries (HIGHEST PRIORITY)
            var isPersonalQuery = _smartGuidanceService.IsPersonalQuery(originalQuery);
            var needsStructuredEarly = _structuredAnswerService.NeedsStructuredAnswer(originalQuery);
            
            _logger.LogInformation("Personal query: {IsPersonal}, Structured: {NeedsStructured} for: {Query}", 
                isPersonalQuery, needsStructuredEarly, originalQuery);
            
            // Skip static ADVICE for personal or structured queries
            if (!isPersonalQuery && !needsStructuredEarly)
            {
                // 7. Handle static intents only if NOT a personal/structured query
                var staticResponse = HandleStaticIntent(intent);
                if (staticResponse != null)
                {
                    _logger.LogInformation("Returning static response for intent: {Intent}", intent);
                    _contextManager.SaveContext(userId, query, intent, query);
                    return staticResponse;
                }
            }
            else
            {
                _logger.LogInformation("Skipping static ADVICE - will use smart guidance or structured answer");
            }

            // 7. Handle comparison queries (NEW - before ResponseFormatter check)
            if (isComparison)
            {
                // Extract entities if not already done
                var entities = _comparisonService.ExtractEntities(query);
                if (entities.HasValue)
                {
                    _contextManager.SaveLastEntities(userId, entities.Value.Entity1, entities.Value.Entity2);
                }
                // Let it continue to RAG+LLM for proper comparison answer
            }

            // 8. Handle old comparison queries (keep for backward compatibility)
            if (ResponseFormatter.IsComparisonQuery(query))
            {
                var comparisonAnswer = ResponseFormatter.HandleComparison(query);
                if (!string.IsNullOrEmpty(comparisonAnswer))
                {
                    return CreateResponse(comparisonAnswer, "Static", 1.0, "COMPARISON");
                }
            }

            // 8. Handle guidance queries
            if (ResponseFormatter.IsGuidanceQuery(query))
            {
                return CreateResponse(ResponseFormatter.HandleGuidance(query), "Static", 1.0, "GUIDANCE");
            }

            // 4. Extract topic from query for context tracking
            var topic = ExtractTopic(query);

            // 4. Check cache (skip cache for expansion queries)
            var cacheKey = $"query_{query.ToLower().Trim()}";
            if (!isExpansion && _cache.TryGetValue<ChatResponse>(cacheKey, out var cachedResponse))
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
                _contextManager.SaveContext(userId, topic, intent, originalQuery);
            }

            // 9. Build context
            var context = BuildContext(topMatches);

            // 9.5. Check for personal query (PRIORITY 1)
            if (isPersonalQuery)
            {
                _logger.LogInformation("Smart guidance mode for personal query: {Query}", originalQuery);
                
                // Check for returns query first
                if (_smartGuidanceService.IsReturnsQuery(originalQuery))
                {
                    var returnsGuidance = _smartGuidanceService.GenerateReturnsGuidance();
                    
                    // Save rich context
                    var returnsEntities = _comparisonService.ExtractAllEntities(originalQuery);
                    _contextManager.SaveRichContext(userId, originalQuery, returnsGuidance, topic, returnsEntities);
                    
                    return CreateResponse(returnsGuidance, "SmartGuidance", 1.0, "GUIDANCE");
                }
                
                // Check for amount-based query
                var amount = _smartGuidanceService.ExtractAmount(originalQuery);
                if (amount > 0)
                {
                    var investmentAdvice = _smartGuidanceService.GenerateInvestmentAdvice(amount);
                    
                    if (!string.IsNullOrEmpty(investmentAdvice))
                    {
                        // Save rich context
                        var amountEntities = _comparisonService.ExtractAllEntities(originalQuery);
                        _contextManager.SaveRichContext(userId, originalQuery, investmentAdvice, topic, amountEntities);
                        
                        return CreateResponse(investmentAdvice, "SmartGuidance", 1.0, "GUIDANCE");
                    }
                }
                
                // Fallback to AI-generated guidance
                var guidedAnswer = await _smartGuidanceService.GenerateGuidedAnswerAsync(originalQuery, context);
                
                // Clean and format
                guidedAnswer = CleanResponse(guidedAnswer);
                guidedAnswer = _personalityService.ApplyPersonality(guidedAnswer);
                
                // Save rich context
                var guidanceEntities = _comparisonService.ExtractAllEntities(originalQuery);
                _contextManager.SaveRichContext(userId, originalQuery, guidedAnswer, topic, guidanceEntities);
                
                // Save to chat history
                await _repository.SaveChatHistoryAsync(new Models.ChatHistory
                {
                    UserId = userId,
                    Role = "Assistant",
                    Message = guidedAnswer,
                    CreatedDate = DateTime.UtcNow
                });

                // Save AI log
                await _repository.SaveAiLogAsync(new Models.AiLog
                {
                    UserId = userId,
                    Query = query,
                    Response = guidedAnswer,
                    ConfidenceScore = topMatches[0].Score,
                    Intent = "GUIDANCE",
                    Source = "SmartGuidance+LLM",
                    CreatedDate = DateTime.UtcNow
                });

                var guidanceResponse = CreateResponse(guidedAnswer, "SmartGuidance+LLM", topMatches[0].Score, "GUIDANCE");
                
                // Cache the response
                var guidanceCacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };
                CacheExtensions.Set(_cache, cacheKey, guidanceResponse, guidanceCacheOptions);
                
                return guidanceResponse;
            }

            // 9.6. Check if structured answer is needed (PRIORITY 2)
            var needsStructured = _structuredAnswerService.NeedsStructuredAnswer(originalQuery);
            
            if (needsStructured)
            {
                _logger.LogInformation("Structured answer mode for: {Query}", originalQuery);
                
                var structuredAnswer = await _structuredAnswerService.GenerateStructuredAnswerAsync(originalQuery, context);
                
                // Clean and format
                structuredAnswer = CleanResponse(structuredAnswer);
                structuredAnswer = _personalityService.ApplyPersonality(structuredAnswer);
                
                // Save rich context
                var structuredEntities = _comparisonService.ExtractAllEntities(originalQuery);
                _contextManager.SaveRichContext(userId, originalQuery, structuredAnswer, topic, structuredEntities);
                
                // Save to chat history
                await _repository.SaveChatHistoryAsync(new Models.ChatHistory
                {
                    UserId = userId,
                    Role = "Assistant",
                    Message = structuredAnswer,
                    CreatedDate = DateTime.UtcNow
                });

                // Save AI log
                await _repository.SaveAiLogAsync(new Models.AiLog
                {
                    UserId = userId,
                    Query = query,
                    Response = structuredAnswer,
                    ConfidenceScore = topMatches[0].Score,
                    Intent = intent,
                    Source = "Structured+LLM",
                    CreatedDate = DateTime.UtcNow
                });

                var structuredResponse = CreateResponse(structuredAnswer, "Structured+LLM", topMatches[0].Score, intent);
                
                // Cache the response
                var structuredCacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };
                CacheExtensions.Set(_cache, cacheKey, structuredResponse, structuredCacheOptions);
                
                return structuredResponse;
            }

            // 10. For high confidence matches AND not expansion mode, return direct answer
            if (topMatches[0].Score > 0.8 && !isExpansion)
            {
                var directAnswer = topMatches[0].Data.Answer;
                
                // Add contextual prefix
                var prefix = ResponseEnhancer.GetContextualPrefix(query);
                var finalAnswer = prefix + directAnswer;
                
                // Format response
                finalAnswer = ResponseFormatter.FormatResponse(finalAnswer);
                
                // Save answer for repetition detection
                _contextManager.SaveLastAnswer(userId, finalAnswer);
                
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

                var dbResponse2 = CreateResponse(finalAnswer, "Database", topMatches[0].Score, intent);
                
                // Cache the response (skip cache for expansion queries)
                var dbCacheOptions2 = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };
                CacheExtensions.Set(_cache, cacheKey, dbResponse2, dbCacheOptions2);
                
                return dbResponse2;
            }

            // 11. For simple definitions with high score AND not expansion, return direct answer
            if (intent == "DEFINITION" && topMatches[0].Score > 0.8 && !isExpansion)
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
            
            // Check for repetition and force expansion
            var richContext = _contextManager.GetRichContext(userId);
            var lastAnswer = richContext?.LastAnswer ?? string.Empty;
            var isFollowUpDetected = _contextManager.IsFollowUpQuery(originalQuery);
            var forceExpansion = isExpansion || (!string.IsNullOrEmpty(lastAnswer) && context.Contains(lastAnswer));
            
            var aiResponse = await _llmService.AskLLMAsync(context, query, chatHistory, forceExpansion, isFollowUpDetected, lastAnswer);

            // Add contextual prefix for natural conversation
            var responsePrefix = ResponseEnhancer.GetContextualPrefix(query);
            aiResponse = responsePrefix + aiResponse;

            // Clean response
            aiResponse = CleanResponse(aiResponse);
            
            // Format response
            aiResponse = ResponseFormatter.FormatResponse(aiResponse);
            
            // Rewrite for natural tone
            aiResponse = await _rewriteService.RewriteAnswerAsync(aiResponse, query);
            
            // Apply personality layer
            aiResponse = _personalityService.ApplyPersonality(aiResponse);
            
            // Save rich context for future follow-ups
            var extractedEntities = _comparisonService.ExtractAllEntities(originalQuery);
            _contextManager.SaveRichContext(userId, originalQuery, aiResponse, topic, extractedEntities);

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
                        return ((KnowledgeData?)null, Score: 0.0);

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
                    return ((KnowledgeData?)null, Score: 0.0);
                }
            })
            .Where(x => x.Item1 != null && x.Score > 0.65)
            .Select(x => (Data: x.Item1!, Score: x.Score))
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
        if (string.IsNullOrWhiteSpace(response))
            return response;

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
        
        // Remove dialogue artifacts (Cuxtomo, Miria, etc.)
        if (response.Contains("Cuxtomo:", StringComparison.OrdinalIgnoreCase) ||
            response.Contains("Miria:", StringComparison.OrdinalIgnoreCase) ||
            response.Contains("Customer:", StringComparison.OrdinalIgnoreCase))
        {
            // Extract only the actual answer, skip dialogue
            var lines = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var answerLines = lines.Where(l => 
                !l.Contains("Cuxtomo:", StringComparison.OrdinalIgnoreCase) &&
                !l.Contains("Miria:", StringComparison.OrdinalIgnoreCase) &&
                !l.Contains("Customer:", StringComparison.OrdinalIgnoreCase) &&
                !l.Contains("Good morning", StringComparison.OrdinalIgnoreCase) &&
                !l.Contains("How may I help", StringComparison.OrdinalIgnoreCase) &&
                l.Length > 20
            ).ToList();
            
            if (answerLines.Any())
            {
                response = string.Join(" ", answerLines);
            }
        }
        
        // Remove prompt artifacts and guidelines
        var artifactKeywords = new[] 
        { 
            "STRICT RULE", "Guidelines:", "Task:", "Your Role:",
            "professional financial content writer", "Keep the exact same information",
            "Maintaine all", "Do not use slaing", "Here's an updated version",
            "Sure! Here's", "updated version of the sentence",
            "Do NOT say", "Instead, start with", "Give simple approach",
            "Show example with numbers", "End with practical tip",
            "User:", "1.", "2.", "3.", "4."
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
                    .Where(s => !s.Contains(":")) // Remove sentences with colons (likely prompts)
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
            response.StartsWith("Good morning", StringComparison.OrdinalIgnoreCase) ||
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
        query = query.ToLower();
        
        // Extract main topic from query with priority order
        if (query.Contains("sip")) return "SIP";
        if (query.Contains("mutual fund")) return "Mutual Fund";
        if (query.Contains("fd") || query.Contains("fixed deposit")) return "Fixed Deposit";
        if (query.Contains("nav")) return "NAV";
        if (query.Contains("stock") || query.Contains("equity")) return "Stock";
        if (query.Contains("bond")) return "Bond";
        if (query.Contains("etf")) return "ETF";
        if (query.Contains("ppf")) return "PPF";
        if (query.Contains("nps")) return "NPS";
        if (query.Contains("gold")) return "Gold";
        if (query.Contains("investment")) return "Investment";
        if (query.Contains("return")) return "Returns";
        if (query.Contains("risk")) return "Risk";
        if (query.Contains("tax")) return "Tax";
        if (query.Contains("withdrawal")) return "Withdrawal";
        if (query.Contains("penalt")) return "Penalty";
        
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

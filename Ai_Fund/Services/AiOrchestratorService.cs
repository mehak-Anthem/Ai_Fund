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
    private readonly IQdrantService _qdrantService;
    private readonly IMfApiService _mfApiService;
    private readonly ICurrencyService _currencyService;
    private readonly IMarketNewsService _marketNewsService;
    private readonly IMarketService _marketService;

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
        ISmartGuidanceService smartGuidanceService,
        IQdrantService qdrantService,
        IMfApiService mfApiService,
        ICurrencyService currencyService,
        IMarketNewsService marketNewsService,
        IMarketService marketService)
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
        _qdrantService = qdrantService;
        _mfApiService = mfApiService;
        _currencyService = currencyService;
        _marketNewsService = marketNewsService;
        _marketService = marketService;
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

            // 1.5 Fetch LIVE MARKET Context (Nifty/Sensex) to align AI with Dashboard
            var marketOverview = await _marketService.GetMarketOverviewAsync();
            var marketJson = JsonSerializer.Serialize(marketOverview);
            _logger.LogInformation("Live Market Data injected into AI context: {MarketData}", marketJson);


            // 2. Normalize input (fix typos)
            query = InputNormalizer.NormalizeInput(query);
            var originalQuery = query;
            _logger.LogInformation("Normalized query: {Query}", query);

            // 2.5. Handle greetings EARLY (before RAG)
            var lowerQuery = query.ToLower().Trim();
            var intent = IntentDetector.DetectIntent(lowerQuery);

            if (lowerQuery == "hi" || lowerQuery == "hello" || lowerQuery == "hey" ||
                lowerQuery.Contains("how are you") || lowerQuery.Contains("how are u") || 
                lowerQuery.Contains("how r you") || lowerQuery.Contains("how r u"))
            {
                var greetingPrompt = "The user is greeting you. Respond warmly as FundAI, a smart mutual fund assistant. Keep it brief and varied.";
                var greetingAnswer = await _llmService.AskLLMAsync(greetingPrompt, originalQuery, new List<ChatMessage>(), false, false, "");
                return CreateResponse(CleanResponse(greetingAnswer), "LLM-Dynamic", 1.0, "GREETING");
            }

            if (lowerQuery.Contains("who are you") || lowerQuery.Contains("who are u") || 
                lowerQuery.Contains("what are you") || lowerQuery.Contains("what are u") ||
                lowerQuery == "who r u" || lowerQuery == "what r u" ||
                lowerQuery.Contains("what") && lowerQuery.Contains("do") && (lowerQuery.Contains("you") || lowerQuery.Contains("u")) ||
                lowerQuery.Contains("identify") || lowerQuery.Contains("your name") ||
                lowerQuery.Contains("what kind of ai") || lowerQuery.Contains("what r you") ||
                lowerQuery.Contains("what can you do") || lowerQuery.Contains("what can u do"))
            {
                var usdToInr = await _currencyService.GetUsdToInrRateAsync();
                var identityPrompt = $"Knowledge Base: I am FundAI, a smart and helpful mutual fund assistant. I can help with SIP calculations, comparing investments (FD vs SIP, etc.), and providing personalized guidance. I also know that 1 USD is currently approx ₹{usdToInr:F1}. I am designed to simplify financial planning for everyone.";
                var identityAnswer = await _llmService.AskLLMAsync(identityPrompt, originalQuery, new List<ChatMessage>(), false, false, "");
                
                identityAnswer = CleanResponse(identityAnswer);
                identityAnswer = _personalityService.ApplyPersonality(identityAnswer);
                
                return CreateResponse(identityAnswer, "LLM-Dynamic", 1.0, "IDENTITY");
            }

            if (lowerQuery.Contains("bye") || lowerQuery.Contains("goodbye") || 
                lowerQuery.Contains("thank you") || lowerQuery.Contains("thanks") ||
                lowerQuery == "quit" || lowerQuery == "exit")
            {
                var closingPrompt = "The user is saying goodbye or thank you. Respond warmly as FundAI, a smart mutual fund assistant. Wish them well on their financial journey. Keep it brief and varied.";
                var closingAnswer = await _llmService.AskLLMAsync(closingPrompt, originalQuery, new List<ChatMessage>(), false, false, "");
                return CreateResponse(CleanResponse(closingAnswer), "LLM-Dynamic", 1.0, "CLOSING");
            }

            if (intent == "CURRENCY")

            {
                var rate = await _currencyService.GetUsdToInrRateAsync();
                var currencyPrompt = $"The user is asking about currency exchange rates. I have the live information that 1 USD is currently approx ₹{rate:F1}. Respond helpfully and mention this live rate.";
                var currencyAnswer = await _llmService.AskLLMAsync(currencyPrompt, originalQuery, new List<ChatMessage>(), false, false, "");
                return CreateResponse(CleanResponse(currencyAnswer), "Live-Currency", 1.0, "CURRENCY");
            }

            var isLiveMarketQuery = _marketNewsService.IsLiveMarketQuery(originalQuery);
            _logger.LogInformation("Live market path check for query: {Query}. IsLiveMarketQuery: {IsLiveMarketQuery}", originalQuery, isLiveMarketQuery);

            if (isLiveMarketQuery)
            {
                var liveArticles = await _marketNewsService.GetLatestMarketNewsAsync(originalQuery);
                _logger.LogInformation("Live market news article count for query: {Query}. Count: {Count}", originalQuery, liveArticles.Count);

                if (liveArticles.Any())
                {
                    var liveContext = _marketNewsService.BuildNewsContext(liveArticles, originalQuery);
                    var marketPrompt = $"{liveContext}\n\nExplain briefly why the market may be falling today. Mention this is based on recent headlines and may evolve during the day.";
                    var marketAnswer = await _llmService.AskLLMAsync(marketPrompt, originalQuery, new List<ChatMessage>(), false, false, "");
                    marketAnswer = CleanResponse(marketAnswer);
                    marketAnswer = _personalityService.ApplyPersonality(marketAnswer);
                    _contextManager.SaveConversationTurn(userId, originalQuery, marketAnswer, "Market News", "MARKET_LIVE", ExtractConversationEntities(originalQuery, marketAnswer));
                    _logger.LogInformation("Returning live market news response for query: {Query}", originalQuery);
                    return CreateResponse(marketAnswer, "Live-Market-News", 1.0, "MARKET_LIVE");
                }

                _logger.LogWarning("Live market query detected but no articles were returned. Falling back to RAG. Query: {Query}", originalQuery);
            }

            // 3. Resolve follow-up queries with context
            var chatHistory = _contextManager.GetRecentChatHistory(userId);
            var existingContext = _contextManager.GetRichContext(userId);
            var isExpansion = _expansionService.IsExpansionQuery(query);
            var isComparison = _comparisonService.IsComparisonQuery(query);
            var isFollowUp = _contextManager.IsFollowUpQuery(query);
            
            if (isComparison)
            {
                var allEntities = _comparisonService.ExtractAllEntities(query);
                if (allEntities.Count >= 2)
                {
                    _contextManager.SaveLastEntities(userId, allEntities[0], allEntities[1]);
                    if (allEntities.Count == 2)
                        query = $"Compare {allEntities[0]} and {allEntities[1]} - explain differences, benefits, and risks";
                    else
                    {
                        var entityList = string.Join(", ", allEntities.Take(allEntities.Count - 1)) + " and " + allEntities.Last();
                        query = $"Compare {entityList} - explain key differences, benefits, and risks of each";
                    }
                }
                else
                {
                    query = _comparisonService.ResolveComparison(query, userId, _contextManager);
                }
            }
            else if (isExpansion)
            {
                var lastTopic = _contextManager.GetLastTopic(userId);
                if (!string.IsNullOrEmpty(lastTopic))
                {
                    query = _expansionService.ExpandQuery(query, lastTopic);
                }
            }
            else
            {
                query = _contextManager.ResolveFollowUp(query, userId);
            }

            // 4. Check for opinion queries
            if (QueryNormalizer.IsOpinionQuery(query))
            {
                var opinionPrompt = "The user is asking for a personal opinion. Explain that as FundAI, you provide factual information and cannot give personal advice.";
                var opinionAnswer = await _llmService.AskLLMAsync(opinionPrompt, originalQuery, new List<ChatMessage>(), false, false, "");
                return CreateResponse(CleanResponse(opinionAnswer), "LLM-Dynamic", 1.0, "OPINION");
            }

            // 5. Normalize query and detect intent
            query = QueryNormalizer.NormalizeQuery(query);
            intent = IntentDetector.DetectIntent(query);

            
            // 6. Check for personal/guidance flags
            var isPersonalQuery = _smartGuidanceService.IsPersonalQuery(originalQuery);
            var needsStructuredEarly = _structuredAnswerService.NeedsStructuredAnswer(originalQuery);

            // 7. Handle static intents
            var staticResponse = HandleStaticIntent(intent, originalQuery);
            if (staticResponse != null) return staticResponse;

            // 8. Cache check
            var cacheKey = $"query_{query.ToLower().Trim()}";
            if (!isExpansion && _cache.TryGetValue<ChatResponse>(cacheKey, out var cachedResponse))
            {
                return cachedResponse;
            }

            // 9. Live Data Lookup (MFAPI) - PRE-SEARCH check
            string liveDataContext = "";
            if (intent == "MF_SPECIFIC" || query.Split(' ').Length > 3)
            {
                var searchTerm = ExtractFundName(query);
                var mfSearch = await _mfApiService.SearchSchemesAsync(searchTerm);
                if (mfSearch != null && mfSearch.Any())
                {
                    var bestScheme = mfSearch.First();
                    var latestNav = await _mfApiService.GetLatestNavAsync(bestScheme.SchemeCode);
                    if (latestNav != null)
                    {
                        liveDataContext = $"\n\nLIVE DATA for {bestScheme.SchemeName} (Code: {bestScheme.SchemeCode}):\nLatest NAV: {latestNav.Nav} as of {latestNav.Date}";
                        _logger.LogInformation(">>> LIVE DATA FOUND: {Fund}", bestScheme.SchemeName);
                    }
                }
            }

            // 9.5. Search and Scale
            var normalizedText = TextNormalizer.Normalize(query);
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(normalizedText);
            var searchResults = await _qdrantService.SearchAsync(queryEmbedding, limit: 3);
            
            var topMatches = searchResults.Select(r => (
                Data: new KnowledgeData 
                { 
                    Id = r.Id, 
                    Question = r.Metadata?.ContainsKey("question") == true ? r.Metadata["question"]?.ToString() ?? "Match" : "Match",
                    Answer = r.Content ?? string.Empty
                }, 
                Score: (double)r.Score
            )).ToList();

            if (!topMatches.Any() && string.IsNullOrEmpty(liveDataContext))
            {
                await _gapService.LogGapAsync(query, intent, 0);
                var clarification = await _llmService.AskLLMAsync("Suggest categories like SIP, comparisons, or fund basics.", originalQuery, new List<ChatMessage>(), false, false, "");
                return CreateResponse(CleanResponse(clarification), "LLM-Dynamic", 0, intent);
            }

            var topic = ExtractTopic(query);
            var richContext = _contextManager.GetRichContext(userId);
            // 10. Handlers (Comparison / Guidance / Structured / RAG)
            if (isComparison)
            {
                var entities = _comparisonService.ExtractAllEntities(query);
                var amount = await _smartGuidanceService.ExtractAmount(originalQuery);
                if (amount == 0 && richContext != null) amount = await _smartGuidanceService.ExtractAmount(richContext.LastUserQuery);
                
                if (entities.Count >= 2 && amount > 0)
                {
                    var comparison = await _smartGuidanceService.CompareInvestmentsAsync(entities[0], entities[1], amount, 1);
                    _contextManager.SaveRichContext(userId, originalQuery, comparison, topic, entities);
                    return CreateResponse(comparison, "SmartGuidance", ScaleConfidence(topMatches[0].Score), "COMPARISON");
                }
            }

            if (isPersonalQuery)
            {
                if (_smartGuidanceService.IsGoalQuery(originalQuery))
                {
                    var goalRes = await _smartGuidanceService.GenerateGoalPlanningAsync(originalQuery);
                    return CreateResponse(goalRes ?? string.Empty, "SmartGuidance-Goal", 1.0, "GOAL");
                }
                
                if (_smartGuidanceService.IsReturnsQuery(originalQuery))
                {
                    var type = _smartGuidanceService.ExtractInvestmentType(originalQuery);
                    var amt = await _smartGuidanceService.ExtractAmount(originalQuery);
                    if (amt > 0 && !string.IsNullOrEmpty(type))
                    {
                        var res = await _smartGuidanceService.GenerateUniversalReturnsAsync(type, amt, 1);
                        return CreateResponse(res, "SmartGuidance", 1.0, "GUIDANCE");
                    }
                }
            }

            // 11. Final RAG+LLM Pipeline
            var liveMarketStatus = await _marketService.GetMarketOverviewAsync();
            var liveMarketContext = $"\n\nCURRENT MARKET STATUS (LIVE INDEX):\n{JsonSerializer.Serialize(liveMarketStatus)}";
            
            var context = BuildContext(topMatches) + liveDataContext + liveMarketContext;
            context = BuildConversationAwareContext(context, richContext, isFollowUp);

            var aiResponse = await _llmService.AskLLMAsync(
                context,
                originalQuery,
                chatHistory,
                isPersonalQuery || needsStructuredEarly,
                isFollowUp,
                existingContext?.LastAnswer);

            aiResponse = CleanResponse(aiResponse);
            aiResponse = _personalityService.ApplyPersonality(aiResponse);

            var guarded = await ApplyGuardrailsDynamic(aiResponse, originalQuery);
            if (guarded != null) return guarded;

            var source = string.IsNullOrEmpty(liveDataContext) ? "RAG+LLM" : "Live+RAG+LLM";
            var topScore = topMatches.Count > 0 ? topMatches[0].Score : 0.9;
            var finalResponse = CreateResponse(aiResponse, source, ScaleConfidence(topScore), intent);

            // Log as a gap if confidence is low (below 0.6) and no live data was used
            if (topScore < 0.6 && string.IsNullOrEmpty(liveDataContext))
            {
                await _gapService.LogGapAsync(originalQuery, intent, topScore);
            }

            var conversationEntities = ExtractConversationEntities(originalQuery, aiResponse);

            _contextManager.SaveConversationTurn(userId, originalQuery, aiResponse, topic, intent, conversationEntities);
            
            // Save and Cache
            var aiLog = new Models.AiLog
            {
                UserId = userId,
                Query = originalQuery,
                Response = aiResponse,
                ConfidenceScore = finalResponse.Confidence,
                Intent = intent,
                Source = source,
                CreatedDate = DateTime.UtcNow
            };

            await _repository.SaveChatHistoryAsync(new Models.ChatHistory { UserId = userId, Role = "Assistant", Message = aiResponse, CreatedDate = DateTime.UtcNow });
            await _repository.SaveAiLogAsync(aiLog);
            CacheExtensions.Set(_cache, cacheKey, finalResponse, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) });


            return finalResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query: {Query}", query);
            return CreateResponse("I'm having a bit of trouble. Please try again later!", "Error", 0, "ERROR");
        }
    }

    private ChatResponse? HandleStaticIntent(string intent, string originalQuery) => null;

    private async Task<ChatResponse?> ApplyGuardrailsDynamic(string aiResponse, string query)
    {
        await Task.CompletedTask;

        if (aiResponse.Contains("guaranteed returns", StringComparison.OrdinalIgnoreCase))
        {
            return CreateResponse("Mutual funds are subject to market risk. No returns are 100% guaranteed.", "SafetyFilter", 0, "BLOCKED");
        }
        return null;
    }

    private string CleanResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response)) return response;
        return response.Replace("SIPI", "SIP").Trim();
    }

    private string ExtractTopic(string query)
    {
        query = query.ToLower();
        if (query.Contains("groww") || query.Contains("et money") || query.Contains("paytm money") ||
            query.Contains("zerodha") || query.Contains("coin") || query.Contains("app"))
            return "Investment App";
        if (query.Contains("market") || query.Contains("nifty") || query.Contains("sensex"))
            return "Market News";
        if (query.Contains("sip")) return "SIP";
        if (query.Contains("mutual fund")) return "Mutual Fund";
        return "Investment";
    }

    private string BuildContext(List<(KnowledgeData Data, double Score)> matches)
    {
        return string.Join("\n\n", matches.Select(m => $"Answer: {m.Data.Answer}"));
    }

    private string BuildConversationAwareContext(string context, ConversationContext? richContext, bool isFollowUp)
    {
        if (!isFollowUp || richContext == null)
        {
            return context;
        }

        var conversationContext =
            $"\n\nRECENT CONVERSATION:\nLast user question: {richContext.LastUserQuery}\nLast assistant answer: {richContext.LastAnswer}";

        if (richContext.LastEntities.Any())
        {
            conversationContext += $"\nReferenced entities: {string.Join(", ", richContext.LastEntities)}";
        }

        return context + conversationContext;
    }

    private List<string> ExtractConversationEntities(string userQuery, string answer)
    {
        var entities = new List<string>();
        var combined = $"{userQuery} {answer}";
        var knownApps = new[] { "Groww", "ET Money", "Paytm Money", "Coin", "Zerodha", "Nifty", "Sensex" };

        foreach (var app in knownApps)
        {
            if (combined.Contains(app, StringComparison.OrdinalIgnoreCase))
            {
                entities.Add(app);
            }
        }

        if (combined.Contains("app", StringComparison.OrdinalIgnoreCase))
        {
            entities.Add("App");
        }

        return entities.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private string ExtractFundName(string query)
    {
        var cleaned = query.ToLower();
        string[] prefixes = { 
            "what is the latest nav of", 
            "what is the nav of", 
            "latest nav of", 
            "nav of", 
            "current price of", 
            "how is", 
            "doing", 
            "search for", 
            "tell me about",
            "show me",
            "what is"
        };

        foreach (var prefix in prefixes)
        {
            if (cleaned.StartsWith(prefix))
            {
                cleaned = cleaned.Substring(prefix.Length).Trim();
            }
            cleaned = cleaned.Replace(prefix, "").Trim();
        }

        // Remove trailing "fund" or "scheme" if present to make search broader
        cleaned = cleaned.Replace("latest nav", "").Trim();
        
        return cleaned;
    }

    private double ScaleConfidence(double rawScore)
    {
        if (rawScore >= 0.8) return 1.0;
        if (rawScore >= 0.7) return 0.9 + (rawScore - 0.7) * 0.5;
        if (rawScore >= 0.6) return 0.75 + (rawScore - 0.6) * 1.5;
        return rawScore;
    }

    private ChatResponse CreateResponse(string answer, string source, double confidence, string intent)
    {
        return new ChatResponse { Answer = answer, Source = source, Confidence = confidence, Intent = intent };
    }
}

using Ai_Fund.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;

namespace Ai_Fund.Services;

public interface IMarketNewsService
{
    bool IsLiveMarketQuery(string query);
    Task<List<MarketAuxArticle>> GetLatestMarketNewsAsync(string query);
    string BuildNewsContext(List<MarketAuxArticle> articles, string userQuery);
}

public class MarketAuxService : IMarketNewsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MarketAuxService> _logger;
    private readonly string _apiToken;

    public MarketAuxService(HttpClient httpClient, IConfiguration configuration, ILogger<MarketAuxService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiToken = configuration["MarketAux:ApiKey"] ?? string.Empty;
        _httpClient.BaseAddress = new Uri(configuration["MarketAux:BaseUrl"] ?? "https://api.marketaux.com");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public bool IsLiveMarketQuery(string query)
    {
        var lower = query.ToLowerInvariant();

        var isMatch = (lower.Contains("market") || lower.Contains("nifty") || lower.Contains("sensex"))
            && (lower.Contains("crash") || lower.Contains("down") || lower.Contains("fall") ||
                lower.Contains("drop") || lower.Contains("today") || lower.Contains("now") ||
                lower.Contains("why"));

        _logger.LogInformation("MarketAux live query detection. Query: {Query}. IsMatch: {IsMatch}", query, isMatch);
        return isMatch;
    }

    public async Task<List<MarketAuxArticle>> GetLatestMarketNewsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(_apiToken) || _apiToken.Contains("YOUR_", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("MarketAux API key is missing. Skipping live market news lookup.");
            return new List<MarketAuxArticle>();
        }

        try
        {
            var publishedAfter = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-ddTHH:mm");
            var searchTerms = BuildSearchTerms(query);
            var parameters = new Dictionary<string, string?>
            {
                ["api_token"] = _apiToken,
                ["language"] = _configuration["MarketAux:Language"] ?? "en",
                ["limit"] = _configuration["MarketAux:Limit"] ?? "5",
                ["must_have_entities"] = "true",
                ["filter_entities"] = "true",
                ["published_after"] = publishedAfter,
                ["search"] = searchTerms
            };

            var countries = _configuration["MarketAux:Countries"];
            if (!string.IsNullOrWhiteSpace(countries))
            {
                parameters["countries"] = countries;
            }

            var url = QueryHelpers.AddQueryString("/v1/news/all", parameters!);
            _logger.LogInformation(
                "Fetching live market news from MarketAux. Query: {Query}. SearchTerms: {SearchTerms}. PublishedAfterUtc: {PublishedAfter}. Countries: {Countries}",
                query,
                searchTerms,
                publishedAfter,
                countries ?? "none");

            var response = await _httpClient.GetFromJsonAsync<MarketAuxResponse>(url);
            var articles = response?.Data?
                .Where(a => !string.IsNullOrWhiteSpace(a.Title))
                .Take(5)
                .ToList() ?? new List<MarketAuxArticle>();

            _logger.LogInformation("MarketAux returned {ArticleCount} articles for query: {Query}", articles.Count, query);
            return articles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching live market news from MarketAux");
            return new List<MarketAuxArticle>();
        }
    }

    public string BuildNewsContext(List<MarketAuxArticle> articles, string userQuery)
    {
        if (articles.Count == 0)
        {
            return string.Empty;
        }

        var lines = new List<string>
        {
            $"LIVE MARKET NEWS CONTEXT for query: {userQuery}",
            $"Timestamp (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
        };

        foreach (var article in articles)
        {
            lines.Add(
                $"- Title: {article.Title}\n" +
                $"  Source: {article.Source}\n" +
                $"  Published: {article.PublishedAt:yyyy-MM-dd HH:mm} UTC\n" +
                $"  Summary: {article.Description}\n" +
                $"  URL: {article.Url}");
        }

        lines.Add("Answer only from the live market context above. If the cause is still uncertain, say the market appears to be reacting to these headlines.");
        return string.Join("\n", lines);
    }

    private static string BuildSearchTerms(string query)
    {
        var lower = query.ToLowerInvariant();

        if (lower.Contains("nifty") || lower.Contains("sensex") || lower.Contains("india") || lower.Contains("indian"))
        {
            return "\"Nifty\" OR \"Sensex\" OR \"Indian stock market\" OR \"India markets\"";
        }

        return "\"stock market\" OR markets OR equities";
    }
}

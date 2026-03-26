using System.Net.Http.Json;
using Ai_Fund.Models;

namespace Ai_Fund.Services;

public class YahooMarketNewsService : IMarketNewsService
{
    private readonly ILogger<YahooMarketNewsService> _logger;

    public YahooMarketNewsService(ILogger<YahooMarketNewsService> logger)
    {
        _logger = logger;
    }

    public bool IsLiveMarketQuery(string query)
    {
        var q = query.ToLower();
        return q.Contains("market") || q.Contains("nifty") || q.Contains("sensex") || q.Contains("falling") || q.Contains("down today");
    }

    public async Task<List<YahooArticle>> GetLatestMarketNewsAsync(string query)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36");
            
            // Search for Nifty/Sensex news specifically for the AI context
            var url = $"https://query2.finance.yahoo.com/v1/finance/search?q=Nifty%20Sensex%20Market&newsCount=3";
            var result = await client.GetFromJsonAsync<YahooSearchResponse>(url);

            if (result?.News != null)
            {
                return result.News.Select(n => new YahooArticle {
                    Uuid = n.Uuid,
                    Title = n.Title,
                    Description = "",
                    Url = n.Link,
                    Source = n.Publisher,
                    ImageUrl = n.Thumbnail?.Resolutions?.FirstOrDefault()?.Url ?? ""
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Yahoo News for AI context.");
        }

        return new List<YahooArticle>();
    }

    public string BuildNewsContext(List<YahooArticle> articles, string query)
    {
        if (articles == null || !articles.Any()) return "";

        var context = "\n\nLATEST MARKET HEADLINES (Yahoo Finance):\n";
        foreach (var art in articles)
        {
            context += $"- {art.Title} (Source: {art.Source})\n";
        }
        return context;
    }
}

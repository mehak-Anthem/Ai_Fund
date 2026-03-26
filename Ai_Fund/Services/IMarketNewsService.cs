namespace Ai_Fund.Services;

public interface IMarketNewsService
{
    bool IsLiveMarketQuery(string query);
    Task<List<YahooArticle>> GetLatestMarketNewsAsync(string query);
    string BuildNewsContext(List<YahooArticle> articles, string query);
}

public class YahooArticle
{
    public string Uuid { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

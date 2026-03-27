using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Ai_Fund.Models;
using System.Collections.Concurrent;

namespace Ai_Fund.Services;

public class MarketService : IMarketService
{
    private readonly ILogger<MarketService> _logger;
    private readonly ICurrencyService _currencyService;
    private static readonly ConcurrentDictionary<string, (object Data, DateTime Timestamp)> _cache = new();
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public MarketService(ILogger<MarketService> logger, ICurrencyService currencyService)
    {
        _logger = logger;
        _currencyService = currencyService;
    }


    public async Task<object> GetMarketOverviewAsync()
    {
        var nifty = await FetchLiveIndexAsync("^NSEI");
        var sensex = await FetchLiveIndexAsync("^BSESN");
        var usdRate = await _currencyService.GetUsdToInrRateAsync();
        
        return new
        {
            nifty,
            sensex,
            usdInr = new { value = $"₹{usdRate:F2}", trend = "Live", color = "indigo" }
        };
    }

    public async Task<object> FetchLiveIndexAsync(string symbol)
    {
        // 1. Check Cache
        if (_cache.TryGetValue(symbol, out var cached) && (DateTime.UtcNow - cached.Timestamp) < CacheDuration)
        {
            return cached.Data;
        }

        try
        {
            using var client = new HttpClient();
            // User-Agent helps avoid some generic bot blocks
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{Uri.EscapeDataString(symbol)}";
            var response = await client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Yahoo Finance rate limit hit for {Symbol}. Serving last known or fallback.", symbol);
                if (cached.Data != null) return cached.Data;
            }
            else
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<YahooFinanceResponse>();
                
                if (result?.Chart?.Result?.Count > 0)
                {
                    var meta = result.Chart.Result[0].Meta;
                    var currentPrice = meta.RegularMarketPrice;
                    var prevClose = meta.ChartPreviousClose;
                    var change = currentPrice - prevClose;
                    var percentChange = (change / prevClose) * 100;
                    
                    var trendPrefix = change >= 0 ? "+" : "";
                    var arrow = change >= 0 ? "↑" : "↓";
                    var color = change >= 0 ? "green" : "rose";
                    
                    var displayName = symbol switch {
                        "^NSEI" => "INDEXNSE: NIFTY_50",
                        "^BSESN" => "INDEXBOM: SENSEX",
                        _ => symbol
                    };

                    var data = new { 
                        symbol = displayName,
                        value = currentPrice.ToString("N2"), 
                        change = $"{trendPrefix}{change:N2}",
                        percent = $"{percentChange:F2}%",
                        trend = $"{trendPrefix}{change:N2} ({percentChange:F2}%) {arrow} today", 
                        color = color,
                        lastUpdate = DateTimeOffset.FromUnixTimeSeconds(meta.RegularMarketTime).ToLocalTime().ToString("dd MMM, h:mm tt") + " IST"
                    };

                    // Update Cache
                    _cache[symbol] = (data, DateTime.UtcNow);
                    return data;
                }
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("429"))
                _logger.LogWarning("Yahoo Finance Rate Limit (429) for {Symbol}.", symbol);
            else
                _logger.LogError(ex, "Error fetching live index for {Symbol}.", symbol);
        }

        var istTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        var lastUpdateStr = istTime.ToString("dd MMM, h:mm tt") + " IST";

        // Final Fallback (if cache empty and API failed)
        return cached.Data ?? (symbol == "^NSEI" 
            ? new { symbol = "INDEXNSE: NIFTY_50", value = "23,206.45", trend = "+294.05 (1.28%) ↑ today", color = "green", lastUpdate = lastUpdateStr }
            : new { symbol = "INDEXBOM: SENSEX", value = "77,456.20", trend = "+412.40 (0.54%) ↑ today", color = "green", lastUpdate = lastUpdateStr });
    }


    public async Task<List<double?>> GetIndexChartAsync(string symbol, string range)
    {
        var cacheKey = $"chart_{symbol}_{range}";
        if (_cache.TryGetValue(cacheKey, out var cached) && (DateTime.UtcNow - cached.Timestamp) < TimeSpan.FromMinutes(5)) // Charts can be older
        {
            return (List<double?>)cached.Data;
        }

        try
        {
            var interval = range switch
            {
                "1d" => "5m",
                "5d" => "15m",
                "1mo" => "1d",
                _ => "1d"
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{Uri.EscapeDataString(symbol)}?range={range}&interval={interval}";
            var response = await client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Yahoo Chart rate limit hit for {Symbol}. Serving cached.", symbol);
                if (cached.Data != null) return (List<double?>)cached.Data;
            }
            else
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<YahooChartResponse>();

                if (result?.Chart?.Result?.Count > 0 && 
                    result.Chart.Result[0].Indicators?.Quote?.Count > 0)
                {
                    var data = result.Chart.Result[0].Indicators.Quote[0].Close;
                    _cache[cacheKey] = (data, DateTime.UtcNow);
                    return data;
                }
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("429"))
                _logger.LogWarning("Yahoo Chart Rate Limit (429) for {Symbol}.", symbol);
            else
                _logger.LogError(ex, "Error fetching chart data for {Symbol} ({Range})", symbol, range);
        }

        return cached.Data != null ? (List<double?>)cached.Data : new List<double?>();
    }

    public async Task<object> GetYahooNewsAsync(string query)
    {
        var cacheKey = $"news_{query}";
        if (_cache.TryGetValue(cacheKey, out var cached) && (DateTime.UtcNow - cached.Timestamp) < TimeSpan.FromMinutes(10))
        {
            return cached.Data;
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36");
            
            var url = $"https://query2.finance.yahoo.com/v1/finance/search?q={Uri.EscapeDataString(query)}&newsCount=4";
            var result = await client.GetFromJsonAsync<YahooSearchResponse>(url);

            if (result?.News != null)
            {
                var articles = result.News.Select(n => new {
                    uuid = n.Uuid,
                    title = n.Title,
                    description = "", // Yahoo search news often doesn't have deep snippets in this response
                    url = n.Link,
                    source = n.Publisher,
                    published_at = DateTimeOffset.FromUnixTimeSeconds(n.ProviderPublishTime).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    image_url = n.Thumbnail?.Resolutions?.FirstOrDefault()?.Url ?? ""
                }).ToList();

                _cache[cacheKey] = (articles, DateTime.UtcNow);
                return articles;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Yahoo News for {Query}", query);
        }

        return new List<object>();
    }
}





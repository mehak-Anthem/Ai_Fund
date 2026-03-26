using System.Text.Json.Serialization;

namespace Ai_Fund.Models;

public class YahooFinanceResponse
{
    [JsonPropertyName("chart")]
    public YahooChart Chart { get; set; } = new();
}

public class YahooChart
{
    [JsonPropertyName("result")]
    public List<YahooResult> Result { get; set; } = new();
}

public class YahooResult
{
    [JsonPropertyName("meta")]
    public YahooMeta Meta { get; set; } = new();
}

public class YahooMeta
{
    [JsonPropertyName("regularMarketPrice")]
    public double RegularMarketPrice { get; set; }

    [JsonPropertyName("chartPreviousClose")]
    public double ChartPreviousClose { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("regularMarketTime")]
    public long RegularMarketTime { get; set; }
}

public class MarketApiResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public MarketDataDetails Data { get; set; } = new();
}

public class MarketDataDetails
{
    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [JsonPropertyName("last_price")]
    public double LastPrice { get; set; }

    [JsonPropertyName("change")]
    public double Change { get; set; }

    [JsonPropertyName("percent_change")]
    public double PercentChange { get; set; }
}


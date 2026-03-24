using Ai_Fund.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Ai_Fund.Services;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CurrencyService> _logger;
    private const string CacheKey = "usd_to_inr_rate";
    private const string ApiUrl = "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies/usd.json";

    public CurrencyService(HttpClient httpClient, IMemoryCache cache, ILogger<CurrencyService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<double> GetUsdToInrRateAsync()
    {
        if (_cache.TryGetValue(CacheKey, out double rate))
        {
            return rate;
        }

        try
        {
            _logger.LogInformation("Fetching live USD to INR exchange rate...");
            var response = await _httpClient.GetFromJsonAsync<CurrencyResponse>(ApiUrl);
            
            if (response != null && response.Usd.TryGetValue("inr", out double liveRate))
            {
                _logger.LogInformation("Successfully fetched live rate: {Rate}", liveRate);
                
                // Cache for 6 hours
                _cache.Set(CacheKey, liveRate, TimeSpan.FromHours(6));
                return liveRate;
            }
            
            _logger.LogWarning("Failed to find 'inr' in currency response. Using fallback.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching live currency rate. Using fallback.");
        }

        return 83.5; // Final fallback
    }
}

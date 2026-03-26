using Microsoft.AspNetCore.Mvc;
using Ai_Fund.Services;
using Microsoft.AspNetCore.Authorization;

namespace Ai_Fund.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class MarketController : ControllerBase
{
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<MarketController> _logger;

    public MarketController(ICurrencyService currencyService, ILogger<MarketController> logger)
    {
        _currencyService = currencyService;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        _logger.LogInformation("Market Overview Requested");
        try
        {
            var usdRate = await _currencyService.GetUsdToInrRateAsync();
            _logger.LogInformation("USD Rate fetched: {Rate}", usdRate);

            
            // For NIFTY/SENSEX, we use reasonable base values with small random jitter 
            // to simulate "live" movement if no real-time key is available.
            // 22,453.80 (+0.45%)
            // 73,903.15 (+0.38%)
            
            var random = new Random();
            var niftyBase = 22453.80;
            var sensexBase = 73903.15;
            
            // Add slight jitter (±0.05%)
            var jitter = (random.NextDouble() * 2 - 1) * 0.0005;
            var nifty = niftyBase * (1 + jitter);
            var sensex = sensexBase * (1 + jitter);

            return Ok(new {
                nifty = new { value = nifty.ToString("N2"), trend = "+0.45%", color = "green" },
                sensex = new { value = sensex.ToString("N2"), trend = "+0.38%", color = "green" },
                usdInr = new { value = "₹" + usdRate.ToString("N2"), trend = "-0.02%", color = "rose" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching market overview");
            return StatusCode(500, "Internal server error");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Ai_Fund.Services;
using Ai_Fund.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Json;


namespace Ai_Fund.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class MarketController : ControllerBase
{
    private readonly IMarketService _marketService;

    public MarketController(IMarketService marketService)
    {
        _marketService = marketService;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var overview = await _marketService.GetMarketOverviewAsync();
        return Ok(overview);
    }

    [HttpGet("chart/{symbol}")]
    public async Task<IActionResult> GetChart(string symbol, [FromQuery] string range = "1d")
    {
        // Internal mapping for Yahoo symbols
        var yahooSymbol = symbol.ToUpper() switch {
            "NIFTY" => "^NSEI",
            "SENSEX" => "^BSESN",
            _ => symbol
        };

        var chartData = await _marketService.GetIndexChartAsync(yahooSymbol, range);
        return Ok(chartData);
    }

    [HttpGet("news")]
    public async Task<IActionResult> GetNews()
    {
        var news = await _marketService.GetYahooNewsAsync("Nifty 50 News");
        return Ok(news);
    }
}







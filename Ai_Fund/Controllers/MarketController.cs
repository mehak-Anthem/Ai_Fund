using Microsoft.AspNetCore.Mvc;
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
}




using Microsoft.AspNetCore.Mvc;
using Ai_Fund.Services;

namespace Ai_Fund.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MutualFundController : ControllerBase
{
    private readonly IMutualFundService _service;

    public MutualFundController(IMutualFundService service)
    {
        _service = service;
    }

    [HttpGet("ask")]
    public async Task<IActionResult> Ask([FromQuery] string query)
    {
        var result = await _service.GetAIAnswerAsync(query);
        return Ok(result);
    }
}

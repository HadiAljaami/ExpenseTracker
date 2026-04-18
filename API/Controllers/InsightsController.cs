using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class InsightsController : BaseController
{
    private readonly IInsightsService _insightsService;

    public InsightsController(IInsightsService insightsService) => _insightsService = insightsService;

    /// <summary>Get financial insights summary for a given month</summary>
    [HttpGet]
    public async Task<IActionResult> GetSummary([FromQuery] int? month, [FromQuery] int? year)
    {
        var m = month ?? DateTime.UtcNow.Month;
        var y = year ?? DateTime.UtcNow.Year;
        var result = await _insightsService.GetSummaryAsync(UserId, m, y);
        return Ok(new { success = true, data = result });
    }
}

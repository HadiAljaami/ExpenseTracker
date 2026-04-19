using Application.Common;
using Application.DTOs.Insights;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/[controller]")]
public class InsightsController : BaseController
{
    private readonly IInsightsService _insightsService;

    public InsightsController(IInsightsService insightsService) => _insightsService = insightsService;

    [HttpGet]
    public async Task<IActionResult> GetSummary([FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await _insightsService.GetSummaryAsync(UserId, month ?? DateTime.UtcNow.Month, year ?? DateTime.UtcNow.Year);
        return Ok(ApiResponse<InsightsSummaryDto>.Ok(result));
    }
}

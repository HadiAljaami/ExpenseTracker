using Application.Common;
using Application.DTOs.Reports;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/[controller]")]
public class ReportsController : BaseController
{
    private readonly IReportService _reportService;
    private readonly IExportService _exportService;

    public ReportsController(IReportService reportService, IExportService exportService)
    {
        _reportService = reportService;
        _exportService = exportService;
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly([FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await _reportService.GetMonthlyReportAsync(UserId, month ?? DateTime.UtcNow.Month, year ?? DateTime.UtcNow.Year);
        return Ok(ApiResponse<MonthlyReportDto>.Ok(result));
    }

    [HttpGet("yearly")]
    public async Task<IActionResult> GetYearly([FromQuery] int? year)
    {
        var result = await _reportService.GetYearlyReportAsync(UserId, year ?? DateTime.UtcNow.Year);
        return Ok(ApiResponse<YearlyReportDto>.Ok(result));
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] int? month, [FromQuery] int? year)
    {
        var m = month ?? DateTime.UtcNow.Month;
        var y = year ?? DateTime.UtcNow.Year;
        var bytes = await _exportService.ExportExpensesToExcelAsync(UserId, m, y);
        var fileName = $"Expenses_{new DateTime(y, m, 1):MMMM_yyyy}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}

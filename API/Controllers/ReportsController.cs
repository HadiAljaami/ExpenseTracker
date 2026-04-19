using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class ReportsController : BaseController
{
    private readonly IReportService _reportService;
    private readonly IExportService _exportService;

    public ReportsController(IReportService reportService, IExportService exportService)
    {
        _reportService = reportService;
        _exportService = exportService;
    }

    /// <summary>Get full monthly report</summary>
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly([FromQuery] int? month, [FromQuery] int? year)
    {
        var m = month ?? DateTime.UtcNow.Month;
        var y = year ?? DateTime.UtcNow.Year;
        var result = await _reportService.GetMonthlyReportAsync(UserId, m, y);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get full yearly report</summary>
    [HttpGet("yearly")]
    public async Task<IActionResult> GetYearly([FromQuery] int? year)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var result = await _reportService.GetYearlyReportAsync(UserId, y);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Export monthly expenses to Excel</summary>
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

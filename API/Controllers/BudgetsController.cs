using Application.DTOs.Budgets;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class BudgetsController : BaseController
{
    private readonly IBudgetService _budgetService;

    public BudgetsController(IBudgetService budgetService) => _budgetService = budgetService;

    /// <summary>Get all budgets for a specific month/year</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? month, [FromQuery] int? year)
    {
        var m = month ?? DateTime.UtcNow.Month;
        var y = year ?? DateTime.UtcNow.Year;
        var result = await _budgetService.GetAllAsync(UserId, m, y);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Create a new budget</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBudgetDto dto)
    {
        var result = await _budgetService.CreateAsync(UserId, dto);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Delete a budget</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _budgetService.DeleteAsync(UserId, id);
        return Ok(new { success = true, message = "Budget deleted successfully." });
    }
}

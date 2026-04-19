using Application.Common;
using Application.DTOs.Budgets;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/[controller]")]
public class BudgetsController : BaseController
{
    private readonly IBudgetService _budgetService;

    public BudgetsController(IBudgetService budgetService) => _budgetService = budgetService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await _budgetService.GetAllAsync(UserId, month ?? DateTime.UtcNow.Month, year ?? DateTime.UtcNow.Year);
        return Ok(ApiResponse<List<BudgetResponseDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBudgetDto dto)
    {
        var result = await _budgetService.CreateAsync(UserId, dto);
        return StatusCode(201, ApiResponse<BudgetResponseDto>.Created(result, "Budget created successfully."));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateBudgetDto dto)
    {
        var result = await _budgetService.UpdateAsync(UserId, id, dto);
        return Ok(ApiResponse<BudgetResponseDto>.Ok(result, "Budget updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _budgetService.DeleteAsync(UserId, id);
        return StatusCode(204, ApiResponse.NoContent("Budget deleted successfully."));
    }
}

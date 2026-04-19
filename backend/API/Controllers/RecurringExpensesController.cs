using Application.Common;
using Application.DTOs.RecurringExpenses;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/recurring-expenses")]
public class RecurringExpensesController : BaseController
{
    private readonly IRecurringExpenseService _recurringService;

    public RecurringExpensesController(IRecurringExpenseService recurringService)
        => _recurringService = recurringService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _recurringService.GetAllAsync(UserId);
        return Ok(ApiResponse<List<RecurringExpenseResponseDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecurringExpenseDto dto)
    {
        var result = await _recurringService.CreateAsync(UserId, dto);
        return StatusCode(201, ApiResponse<RecurringExpenseResponseDto>.Created(result, "Recurring expense created."));
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var result = await _recurringService.ToggleActiveAsync(UserId, id);
        var status = result.IsActive ? "activated" : "deactivated";
        return Ok(ApiResponse<RecurringExpenseResponseDto>.Ok(result, $"Recurring expense {status}."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _recurringService.DeleteAsync(UserId, id);
        return StatusCode(204, ApiResponse.NoContent("Recurring expense deleted."));
    }
}

using Application.DTOs.RecurringExpenses;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/recurring-expenses")]
public class RecurringExpensesController : BaseController
{
    private readonly IRecurringExpenseService _recurringService;

    public RecurringExpensesController(IRecurringExpenseService recurringService)
        => _recurringService = recurringService;

    /// <summary>Get all recurring expenses</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _recurringService.GetAllAsync(UserId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Create a new recurring expense</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecurringExpenseDto dto)
    {
        var result = await _recurringService.CreateAsync(UserId, dto);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Toggle active/inactive status</summary>
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var result = await _recurringService.ToggleActiveAsync(UserId, id);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Delete a recurring expense</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _recurringService.DeleteAsync(UserId, id);
        return Ok(new { success = true, message = "Recurring expense deleted." });
    }
}

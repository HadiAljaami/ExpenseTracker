using Application.DTOs.Expenses;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class ExpensesController : BaseController
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService) => _expenseService = expenseService;

    /// <summary>Get all expenses with filtering, sorting, and pagination</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ExpenseFilterDto filter)
    {
        var result = await _expenseService.GetAllAsync(UserId, filter);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get a single expense by ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _expenseService.GetByIdAsync(UserId, id);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Create a new expense</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto)
    {
        var result = await _expenseService.CreateAsync(UserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    /// <summary>Update an existing expense</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseDto dto)
    {
        var result = await _expenseService.UpdateAsync(UserId, id, dto);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Delete an expense</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _expenseService.DeleteAsync(UserId, id);
        return Ok(new { success = true, message = "Expense deleted successfully." });
    }
}

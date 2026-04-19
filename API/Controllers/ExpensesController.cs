using Application.Common;
using Application.DTOs.Expenses;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/[controller]")]
public class ExpensesController : BaseController
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService) => _expenseService = expenseService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ExpenseFilterDto filter)
    {
        var result = await _expenseService.GetAllAsync(UserId, filter);
        return Ok(ApiResponse<PagedExpensesDto>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _expenseService.GetByIdAsync(UserId, id);
        return Ok(ApiResponse<ExpenseResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto)
    {
        var result = await _expenseService.CreateAsync(UserId, dto);
        return StatusCode(201, ApiResponse<ExpenseResponseDto>.Created(result, "Expense created successfully."));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseDto dto)
    {
        var result = await _expenseService.UpdateAsync(UserId, id, dto);
        return Ok(ApiResponse<ExpenseResponseDto>.Ok(result, "Expense updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _expenseService.DeleteAsync(UserId, id);
        return StatusCode(204, ApiResponse.NoContent("Expense deleted successfully."));
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeleted()
    {
        var result = await _expenseService.GetDeletedAsync(UserId);
        return Ok(ApiResponse<List<ExpenseResponseDto>>.Ok(result));
    }

    [HttpPatch("{id}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        await _expenseService.RestoreAsync(UserId, id);
        return Ok(ApiResponse.Ok("Expense restored successfully."));
    }
}

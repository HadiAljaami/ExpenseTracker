using Application.Common;
using Application.DTOs.Admin;
using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[Route("api/v1/admin")]
[Authorize(Roles = UserRoles.Admin)]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService) => _adminService = adminService;

    private int AdminId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string AdminEmail => User.FindFirstValue(ClaimTypes.Email) ?? "";
    private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();

    // ─── Users ────────────────────────────────────────────────────────────────

    /// <summary>Get all users with filtering, search and pagination</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] AdminUserFilterDto filter)
    {
        var result = await _adminService.GetAllUsersAsync(filter);
        return Ok(ApiResponse<PagedAdminUsersDto>.Ok(result));
    }

    /// <summary>Get specific user details</summary>
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _adminService.GetUserByIdAsync(id);
        return Ok(ApiResponse<AdminUserDto>.Ok(result));
    }

    /// <summary>Create a new user from admin panel</summary>
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateAdminUserDto dto)
    {
        var result = await _adminService.CreateUserAsync(dto);
        return StatusCode(201, ApiResponse<AdminUserDto>.Created(result, "User created successfully."));
    }

    /// <summary>Change user role (User / Admin)</summary>
    [HttpPatch("users/{id}/role")]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeUserRoleDto dto)
    {
        var result = await _adminService.ChangeUserRoleAsync(AdminId, AdminEmail, id, dto.Role, IpAddress);
        return Ok(ApiResponse<AdminUserDto>.Ok(result, $"User role changed to {dto.Role}."));
    }

    /// <summary>Suspend or unsuspend a user account</summary>
    [HttpPatch("users/{id}/suspend")]
    public async Task<IActionResult> ToggleSuspend(int id)
    {
        var result = await _adminService.ToggleSuspendUserAsync(AdminId, AdminEmail, id, IpAddress);
        var status = result.IsSuspended ? "suspended" : "unsuspended";
        return Ok(ApiResponse<AdminUserDto>.Ok(result, $"User {status} successfully."));
    }

    /// <summary>Delete a user account permanently</summary>
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _adminService.DeleteUserAsync(AdminId, AdminEmail, id, IpAddress);
        return StatusCode(204, ApiResponse.NoContent("User deleted successfully."));
    }

    // ─── Expenses ─────────────────────────────────────────────────────────────

    /// <summary>Get all expenses across all users with filtering</summary>
    [HttpGet("expenses")]
    public async Task<IActionResult> GetAllExpenses([FromQuery] AdminExpenseFilterDto filter)
    {
        var result = await _adminService.GetAllExpensesAsync(filter);
        return Ok(ApiResponse<PagedAdminExpensesDto>.Ok(result));
    }

    /// <summary>Delete any expense from admin panel</summary>
    [HttpDelete("expenses/{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        await _adminService.DeleteExpenseAsync(AdminId, AdminEmail, id, IpAddress);
        return StatusCode(204, ApiResponse.NoContent("Expense deleted successfully."));
    }

    // ─── Reports & Stats ──────────────────────────────────────────────────────

    /// <summary>Get system-wide statistics dashboard</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _adminService.GetSystemStatsAsync();
        return Ok(ApiResponse<AdminStatsDto>.Ok(result));
    }

    /// <summary>Get monthly growth data for a specific year</summary>
    [HttpGet("stats/monthly")]
    public async Task<IActionResult> GetMonthlyGrowth([FromQuery] int? year)
    {
        var result = await _adminService.GetMonthlyGrowthAsync(year ?? DateTime.UtcNow.Year);
        return Ok(ApiResponse<List<SystemMonthlyDto>>.Ok(result));
    }

    /// <summary>Get full system report for a specific month</summary>
    [HttpGet("reports")]
    public async Task<IActionResult> GetSystemReport([FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await _adminService.GetSystemReportAsync(
            month ?? DateTime.UtcNow.Month,
            year ?? DateTime.UtcNow.Year);
        return Ok(ApiResponse<AdminSystemReportDto>.Ok(result));
    }

    // ─── Audit Log ────────────────────────────────────────────────────────────

    /// <summary>Get admin audit log with filtering</summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? adminId = null,
        [FromQuery] string? action = null)
    {
        var result = await _adminService.GetAuditLogsAsync(page, pageSize, adminId, action);
        return Ok(ApiResponse<PagedAuditLogsDto>.Ok(result));
    }
}

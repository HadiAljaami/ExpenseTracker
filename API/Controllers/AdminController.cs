using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/admin")]
[Authorize(Roles = "Admin")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService) => _adminService = adminService;

    /// <summary>Get all users with their stats</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _adminService.GetAllUsersAsync();
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get specific user details</summary>
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _adminService.GetUserByIdAsync(id);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Delete a user account</summary>
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _adminService.DeleteUserAsync(id);
        return Ok(new { success = true, message = "User deleted successfully." });
    }

    /// <summary>Get system-wide statistics</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _adminService.GetSystemStatsAsync();
        return Ok(new { success = true, data = result });
    }
}

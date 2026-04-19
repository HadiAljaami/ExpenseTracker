using Application.DTOs.Users;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/users")]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    /// <summary>Get current user profile</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userService.GetProfileAsync(UserId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Update current user's full name</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var result = await _userService.UpdateProfileAsync(UserId, dto);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Change current user's password</summary>
    [HttpPut("me/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        await _userService.ChangePasswordAsync(UserId, dto);
        return Ok(new { success = true, message = "Password changed successfully." });
    }

    /// <summary>Delete current user account permanently</summary>
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount()
    {
        await _userService.DeleteAccountAsync(UserId);
        return Ok(new { success = true, message = "Account deleted successfully." });
    }
}

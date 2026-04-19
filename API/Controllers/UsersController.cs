using Application.Common;
using Application.DTOs.Users;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/users")]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userService.GetProfileAsync(UserId);
        return Ok(ApiResponse<UserProfileDto>.Ok(result));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var result = await _userService.UpdateProfileAsync(UserId, dto);
        return Ok(ApiResponse<UserProfileDto>.Ok(result, "Profile updated successfully."));
    }

    [HttpPut("me/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        await _userService.ChangePasswordAsync(UserId, dto);
        return Ok(ApiResponse.Ok("Password changed successfully."));
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount()
    {
        await _userService.DeleteAccountAsync(UserId);
        return StatusCode(204, ApiResponse.NoContent("Account deleted successfully."));
    }
}

using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/[controller]")]
public class AlertsController : BaseController
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService) => _alertService = alertService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? unreadOnly = null)
    {
        var result = await _alertService.GetAlertsAsync(UserId, page, pageSize, unreadOnly);
        return Ok(ApiResponse<PagedAlertsDto>.Ok(result));
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _alertService.MarkAsReadAsync(UserId, id);
        return Ok(ApiResponse.Ok("Alert marked as read."));
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _alertService.MarkAllAsReadAsync(UserId);
        return Ok(ApiResponse.Ok("All alerts marked as read."));
    }
}

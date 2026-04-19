using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class AlertsController : BaseController
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService) => _alertService = alertService;

    /// <summary>Get paged alerts (use unreadOnly=true for unread only)</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? unreadOnly = null)
    {
        var result = await _alertService.GetAlertsAsync(UserId, page, pageSize, unreadOnly);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Mark a specific alert as read</summary>
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _alertService.MarkAsReadAsync(UserId, id);
        return Ok(new { success = true, message = "Alert marked as read." });
    }

    /// <summary>Mark all alerts as read</summary>
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _alertService.MarkAllAsReadAsync(UserId);
        return Ok(new { success = true, message = "All alerts marked as read." });
    }
}

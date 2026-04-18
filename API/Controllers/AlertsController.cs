using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class AlertsController : BaseController
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService) => _alertService = alertService;

    /// <summary>Get all alerts (read and unread)</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _alertService.GetAllAlertsAsync(UserId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get unread alerts only</summary>
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var result = await _alertService.GetUnreadAlertsAsync(UserId);
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

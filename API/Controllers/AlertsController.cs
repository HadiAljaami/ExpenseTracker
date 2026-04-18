using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class AlertsController : BaseController
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService) => _alertService = alertService;

    /// <summary>Get all unread alerts for the current user</summary>
    [HttpGet]
    public async Task<IActionResult> GetUnread()
    {
        var result = await _alertService.GetUnreadAlertsAsync(UserId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Mark an alert as read</summary>
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _alertService.MarkAsReadAsync(UserId, id);
        return Ok(new { success = true, message = "Alert marked as read." });
    }
}

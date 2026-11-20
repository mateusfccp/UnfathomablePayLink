namespace UnfathomablePayLink.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnfathomablePayLink.Authentication;
using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Exposes endpoints for inspecting, configuring, and removing the authenticated <see cref="Business"/> webhook URL.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.BusinessOnly)]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class WebhookController : ControllerBase
{
    private readonly IBusinessService _businessService;
    private readonly ILogger<WebhookController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookController"/> class.
    /// </summary>
    /// <param name="businessService">The service used to manage business webhook settings.</param>
    /// <param name="logger">The logger used to record webhook configuration endpoint requests.</param>
    public WebhookController(
        IBusinessService businessService,
        ILogger<WebhookController> logger)
    {
        _businessService = businessService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the webhook URL configured for the authenticated business.
    /// </summary>
    /// <returns>An <see cref="ActionResult{TValue}"/> containing the current <see cref="WebhookDto"/>.</returns>
    /// <response code="200">Returns the configured webhook URL (or null if none is set).</response>
    /// <response code="401">Missing or invalid API Key.</response>
    [HttpGet]
    [ProducesResponseType<WebhookDto>(StatusCodes.Status200OK)]
    public ActionResult<WebhookDto> Get()
    {
        var business = HttpContext.Features.Get<Business>()!;
        _logger.LogDebug(
            "Retrieved webhook configuration for business {BusinessId} (IsConfigured={IsConfigured}).",
            business.Id,
            !string.IsNullOrWhiteSpace(business.WebhookUrl));
        return Ok(new WebhookDto { Url = business.WebhookUrl });
    }

    /// <summary>
    /// Configures or updates the webhook URL for the authenticated business.
    /// </summary>
    /// <remarks>
    /// Whenever a payment reaches a terminal state (<c>APPROVED</c> or <c>REJECTED</c>), PayLink sends an HTTP <c>POST</c>
    /// request with <c>Content-Type: application/json</c> containing a <see cref="PaymentDto"/> payload to the configured
    /// URL.
    /// </remarks>
    /// <param name="dto">The payload containing the new webhook URL.</param>
    /// <returns>An <see cref="ActionResult{TValue}"/> containing the updated <see cref="WebhookDto"/>.</returns>
    /// <response code="200">Webhook URL updated successfully.</response>
    /// <response code="400">Missing, empty, or invalid webhook URL.</response>
    /// <response code="401">Missing or invalid API Key.</response>
    /// <response code="404">Authenticated business was not found.</response>
    [HttpPut]
    [Consumes("application/json")]
    [ProducesResponseType<WebhookDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebhookDto>> Update([FromBody] WebhookUpdateDto dto)
    {
        var business = HttpContext.Features.Get<Business>()!;

        if (string.IsNullOrWhiteSpace(dto.Url))
        {
            _logger.LogWarning(
                "Rejected webhook update request from business {BusinessId}: empty URL.",
                business.Id);
            return BadRequest(new { message = "Webhook URL is required." });
        }

        try
        {
            var updated = await _businessService.UpdateWebhookUrlAsync(business.Id, dto.Url);
            if (updated == null)
            {
                return NotFound();
            }

            return Ok(new WebhookDto { Url = updated.WebhookUrl });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Removes the webhook URL configured for the authenticated business.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the removal.</returns>
    /// <response code="204">Webhook URL removed successfully (or already unset).</response>
    /// <response code="401">Missing or invalid API Key.</response>
    /// <response code="404">Authenticated business was not found.</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete()
    {
        var business = HttpContext.Features.Get<Business>()!;

        var removed = await _businessService.RemoveWebhookUrlAsync(business.Id);
        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }
}

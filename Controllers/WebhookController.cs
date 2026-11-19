namespace UnfathomablePayLink.Controllers;

using Microsoft.AspNetCore.Mvc;
using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class WebhookController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public WebhookController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    /// <summary>
    /// Retrieves the webhook URL configured for the authenticated business.
    /// </summary>
    /// <response code="200">Returns the configured webhook URL (or null if none is set).</response>
    /// <response code="401">Missing or invalid API Key.</response>
    [HttpGet]
    [ProducesResponseType<WebhookDto>(StatusCodes.Status200OK)]
    public ActionResult<WebhookDto> Get()
    {
        var business = HttpContext.Features.Get<Business>()!;
        return Ok(new WebhookDto { Url = business.WebhookUrl });
    }

    /// <summary>
    /// Configures or updates the webhook URL for the authenticated business.
    /// </summary>
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
        if (string.IsNullOrWhiteSpace(dto.Url))
        {
            return BadRequest(new { message = "Webhook URL is required." });
        }

        var business = HttpContext.Features.Get<Business>()!;

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

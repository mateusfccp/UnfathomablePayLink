namespace UnfathomablePayLink.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnfathomablePayLink.Authentication;
using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Exceptions;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Exposes administrator-only CRUD endpoints for managing registered <see cref="Business"/> entities.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class BusinessesController : ControllerBase
{
    private readonly IBusinessService _businessService;
    private readonly Converter<Business, BusinessDto> _businessConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessesController"/> class.
    /// </summary>
    /// <param name="businessService">The service used to manage business entities.</param>
    /// <param name="businessConverter">The converter used to map <see cref="Business"/> entities to <see cref="BusinessDto"/> instances.</param>
    public BusinessesController(
        IBusinessService businessService,
        Converter<Business, BusinessDto> businessConverter)
    {
        _businessService = businessService;
        _businessConverter = businessConverter;
    }

    /// <summary>
    /// Retrieves all registered businesses.
    /// </summary>
    /// <returns>An <see cref="ActionResult{TValue}"/> containing all <see cref="BusinessDto"/> records.</returns>
    /// <response code="200">Returns the list of registered businesses.</response>
    /// <response code="401">Missing or invalid API Key.</response>
    /// <response code="403">Caller is not authenticated as an administrator.</response>
    [HttpGet]
    [ProducesResponseType<IEnumerable<BusinessDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BusinessDto>>> GetAll()
    {
        var businesses = await _businessService.GetAllBusinessesAsync();
        return Ok(businesses.ConvertAll(_businessConverter));
    }

    /// <summary>
    /// Retrieves a specific business by its primary key identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the business.</param>
    /// <returns>An <see cref="ActionResult{TValue}"/> containing the requested <see cref="BusinessDto"/>.</returns>
    /// <response code="200">Returns the requested business.</response>
    /// <response code="401">Missing or invalid API Key.</response>
    /// <response code="403">Caller is not authenticated as an administrator.</response>
    /// <response code="404">Business not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<BusinessDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessDto>> GetById(int id)
    {
        var business = await _businessService.GetBusinessByIdAsync(id);
        if (business is null)
        {
            return NotFound(new { message = $"Business with ID {id} was not found." });
        }

        return Ok(_businessConverter(business));
    }

    /// <summary>
    /// Registers a new business and generates its unique API key on the server.
    /// </summary>
    /// <param name="dto">The business creation payload.</param>
    /// <returns>An <see cref="ActionResult{TValue}"/> containing the created <see cref="BusinessDto"/>.</returns>
    /// <response code="201">Business created successfully.</response>
    /// <response code="400">Invalid payload, webhook URL, or invoice provider key.</response>
    /// <response code="401">Missing or invalid API Key.</response>
    /// <response code="403">Caller is not authenticated as an administrator.</response>
    /// <response code="409">A business with the same document already exists.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<BusinessDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BusinessDto>> Create([FromBody] BusinessCreateDto dto)
    {
        try
        {
            var created = await _businessService.CreateBusinessAsync(dto);
            var resultDto = _businessConverter(created);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }
        catch (BusinessConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing business while preserving its server-generated API key.
    /// </summary>
    /// <param name="id">The unique identifier of the business to update.</param>
    /// <param name="dto">The business update payload.</param>
    /// <returns>An <see cref="ActionResult{TValue}"/> containing the updated <see cref="BusinessDto"/>.</returns>
    /// <response code="200">Business updated successfully.</response>
    /// <response code="400">Invalid payload, webhook URL, or invoice provider key.</response>
    /// <response code="401">Missing or invalid API Key.</response>
    /// <response code="403">Caller is not authenticated as an administrator.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="409">Another business already uses the specified document.</response>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType<BusinessDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BusinessDto>> Update(int id, [FromBody] BusinessUpdateDto dto)
    {
        try
        {
            var updated = await _businessService.UpdateBusinessAsync(id, dto);
            if (updated is null)
            {
                return NotFound(new { message = $"Business with ID {id} was not found." });
            }

            return Ok(_businessConverter(updated));
        }
        catch (BusinessConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a business by its identifier if it has no associated payments.
    /// </summary>
    /// <param name="id">The unique identifier of the business to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the deletion.</returns>
    /// <response code="204">Business deleted successfully.</response>
    /// <response code="401">Missing or invalid API Key.</response>
    /// <response code="403">Caller is not authenticated as an administrator.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="409">Business has associated payments and cannot be deleted.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _businessService.DeleteBusinessAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Business with ID {id} was not found." });
            }

            return NoContent();
        }
        catch (BusinessConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}

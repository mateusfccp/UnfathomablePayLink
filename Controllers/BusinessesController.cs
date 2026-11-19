namespace UnfathomablePayLink.Controllers;

using Microsoft.AspNetCore.Mvc;
using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class BusinessesController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessesController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    /// <summary>
    /// GET api/businesses
    /// Lists all registered businesses mapped to DTOs.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BusinessDto>>> GetAll()
    {
        var businesses = await _businessService.GetAllBusinessesAsync();
        return Ok(businesses.Select(b => b.ToDto()));
    }

    /// <summary>
    /// GET api/businesses/{id}
    /// Gets a single business by ID mapped to a DTO.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BusinessDto>> GetById(int id)
    {
        var business = await _businessService.GetBusinessByIdAsync(id);
        if (business == null)
        {
            return NotFound(new { message = $"Business with ID {id} not found." });
        }

        return Ok(business.ToDto());
    }

    /// <summary>
    /// POST api/businesses
    /// Registers a new business entity using an input DTO.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BusinessDto>> Create([FromBody] BusinessCreateDto dto)
    {
        try
        {
            var business = dto.ToEntity();
            var created = await _businessService.RegisterBusinessAsync(business);
            var resultDto = created.ToDto();
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PUT api/businesses/{id}
    /// Updates an existing business using an input DTO.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BusinessDto>> Update(int id, [FromBody] BusinessCreateDto dto)
    {
        try
        {
            var business = dto.ToEntity();
            var updated = await _businessService.UpdateBusinessAsync(id, business);
            if (updated == null)
            {
                return NotFound(new { message = $"Business with ID {id} not found." });
            }

            return Ok(updated.ToDto());
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// DELETE api/businesses/{id}
    /// Deletes a business by ID.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _businessService.DeleteBusinessAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Business with ID {id} not found." });
        }

        return NoContent();
    }
}

namespace UnfathomablePayLink.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnfathomablePayLink.Context;
using UnfathomablePayLink.Models;

[ApiController]
[Route("api/[controller]")]
public class BusinessesController : ControllerBase
{
    private readonly PayLinkContext _context;

    public BusinessesController(PayLinkContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET api/businesses
    /// Lists all registered businesses.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Business>>> GetAll()
    {
        return Ok(await _context.Businesses.ToListAsync());
    }

    /// <summary>
    /// GET api/businesses/{id}
    /// Gets a single business by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Business>> GetById(int id)
    {
        var business = await _context.Businesses.FindAsync(id);
        if (business == null)
        {
            return NotFound(new { message = $"Business with ID {id} not found." });
        }

        return Ok(business);
    }

    /// <summary>
    /// POST api/businesses
    /// Registers a new business entity.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Business>> Create(Business business)
    {
        // Check document uniqueness constraint
        bool documentExists = await _context.Businesses.AnyAsync(b => b.Document == business.Document);
        if (documentExists)
        {
            return BadRequest(new { message = $"A business with document '{business.Document}' already exists." });
        }

        _context.Businesses.Add(business);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = business.Id }, business);
    }
}
using Clinic.Core.Contracts;
using Clinic.Core.Dto;
using Clinic.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("/v1/api/visits")]
public class VisitsController : ControllerBase
{
    private readonly IVisitRepository _visitRepository;

    public VisitsController(IVisitRepository _visitRepository)
    {
        this._visitRepository = _visitRepository;
    }

    [HttpGet(Name = "GetAllVisitsAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<VisitResponse>>> GetAllAsync()
    {
        var visits = await _visitRepository.GetAllAsync();

        if (!visits.Any()) return Ok(Enumerable.Empty<VisitResponse>());

        return Ok(visits);
    }

    [HttpGet("{id}", Name = "GetVisitAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisitResponse>> GetAsync(int id)
    {
        var visit = await _visitRepository.GetByIdAsync(id);
        if (visit == null) return NotFound();

        return Ok(visit);
    }

    [HttpPost(Name = "CreateVisit")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync(VisitDto VisitDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Message = "Invalid input data", Errors = ModelState });
        }

        try
        {
            var visit = await _visitRepository.CreateAsync(VisitDto);

            if (visit != null)
            {
                return Created("Created", visit);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Failed to create visit" });
        }
        catch (Exception ex)
        {
            // Logging in the future
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = "An error occurred while creating the visit." });
        }
    }
}
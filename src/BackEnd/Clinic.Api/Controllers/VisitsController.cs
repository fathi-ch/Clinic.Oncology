using Clinic.Api.BusinessService;
using Clinic.Core.Contracts;
using Clinic.Core.Dto;
using Clinic.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("/v1/api/visits")]
public class VisitsController : ControllerBase
{
    private readonly IVisitsService _visitRepository;
    private readonly IDocumentRepository _documentRepository;


    public VisitsController(IVisitsService visitRepository, IDocumentRepository documentRepository)
    {
        this._visitRepository = visitRepository;
        this._documentRepository = documentRepository;
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

    [HttpGet("{fromDate}/{toDate}", Name = "GetBydDateAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<VisitResponse>>> GetByDateAsync([FromRoute] DateTime fromDate, [FromRoute] DateTime toDate)
    {
        var visits = await _visitRepository.GetBydDateAsync(fromDate, toDate);

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
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync(VisitDto VisitDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Message = "Invalid input data", Errors = ModelState });
        }

        var refusal = await ValidateBookingAsync(VisitDto);
        if (refusal != null) return refusal;

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


    [HttpGet("{visitid}/documents", Name = "GetDocumentsByVisitIdAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientDocumentResponse>> GetDocumentsByVisitIdAsync(int visitid)
    {
        var patientDocumentResponse = await _documentRepository.GetByVisitIdAsync(visitid);
        if (patientDocumentResponse == null) return NotFound();

        return Ok(patientDocumentResponse);
    }

   

    [HttpDelete("{id}", Name = "DeleteVisit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteByIdAsync(int id)
    {
        try
        {
             await _visitRepository.DeleteByIdAsync(id);
             return Ok();
        }
        catch (Exception ex)
        {
            // Log the exception (ex) here
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = "An error occurred while deleting the Visit" });
        }
    }

    [HttpPut("{id}", Name = "UpdateVisitAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VisitResponse>> UpdateAsync(int id, VisitDto visitDto)
    {
        var visit = await _visitRepository.GetByIdAsync(id);
        if (visit == null) return NotFound();

        // a finished visit is a medical record: it can be read and exported, not changed or restarted
        if (visit.Status == "TRM")
            return Conflict(new { Message = "Cette visite est terminée et ne peut plus être modifiée." });

        var refusal = await ValidateBookingAsync(visitDto, id);
        if (refusal != null) return refusal;

        var visitInDb = await _visitRepository.UpdateByIdAsync(id, visitDto);

        return Ok(visitInDb);
    }

    // A booking must have a known billing type and a valid price, end after it starts and not overlap another visit.
    private static readonly string[] BillingTypes = { "PAID", "PROMO", "FREE" };

    private async Task<ObjectResult?> ValidateBookingAsync(VisitDto visitDto, int exceptVisitId = 0)
    {
        visitDto.BillingType = string.IsNullOrWhiteSpace(visitDto.BillingType) ? "PAID" : visitDto.BillingType.ToUpperInvariant();
        if (!BillingTypes.Contains(visitDto.BillingType))
            return BadRequest(new { Message = "Type de facturation inconnu (PAID, PROMO ou FREE)." });
        if (visitDto.BillingType == "FREE")
            visitDto.Price = 0; // a free visit never counts as income, whatever price was typed
        if (visitDto.Price < 0)
            return BadRequest(new { Message = "Le prix ne peut pas être négatif." });

        if (visitDto.EndTime <= visitDto.StartTime)
            return BadRequest(new { Message = "L'heure de fin doit être après l'heure de début." });

        if (await _visitRepository.HasOverlapAsync(visitDto.StartTime, visitDto.EndTime, exceptVisitId))
            return Conflict(new { Message = "Ce créneau est déjà réservé." });

        return null;
    }
}

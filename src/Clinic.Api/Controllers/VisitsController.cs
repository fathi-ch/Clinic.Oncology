﻿using Clinic.Api.BusinessService;
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisitResponse>> UpdateAsync(int id, VisitDto visitDto)
    {
        var visit = await _visitRepository.GetByIdAsync(id);
        if (visit == null) return NotFound();

        var visitInDb = await _visitRepository.UpdateByIdAsync(id, visitDto);

        return Ok(visitInDb);
    }
}
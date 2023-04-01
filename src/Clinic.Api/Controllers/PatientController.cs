using Clinic.Core.Contracts;
using Clinic.Core.Mappers;
using Clinic.Core.Models;
using Clinic.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("/v1/[controller]")]
public class PatientController : ControllerBase
{
    private readonly IPatientRepository _patient;
    
    public PatientController(IPatientRepository patient)
    {
        _patient = patient;
    }
    
    [HttpGet(Name = "GetAllPatients")]
    public async Task<ActionResult<IEnumerable<PatientResponse>>> GetAll()
    {
        var result = await _patient.GetAllAsync();
        var patients = result.ToList();
        if (!patients.Any())
        {
            return NotFound();
        }
        return Ok(patients.Select(p => p.ToPatientResponse()));
    }
    
    [HttpGet("{id}",Name = "GetPatient")]
    public async Task<ActionResult<PatientResponse>> Get(string id)
    {
        var patient = await _patient.GetByIdAsync(id);
        if (patient == null)
        {
            return NotFound();
        }
        return Ok(patient.ToPatientResponse());
    }

    [HttpPost(Name = "CreateWithDocuments")]
    public async Task<IActionResult> CreateWithDocumentsAsync([FromBody]Patient patient,[FromForm] IEnumerable<IFormFile> files)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
 
        var result = await _patient.CreateWithDocumentsAsync(patient, files);
 
        if (result)
        {
            return Created("Created", result);
        }
 
        return StatusCode(StatusCodes.Status500InternalServerError);
    }
}
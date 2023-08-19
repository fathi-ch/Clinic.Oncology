using Clinic.Core.Contracts;
using Clinic.Core.Mappers;
using Clinic.Core.Models;
using Clinic.Core.Repositories;
using Clinic.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("/v1/api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientRepository _patientRepository;

    public PatientsController(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository ??
                             throw new ArgumentException(null, nameof(patientRepository));
    }

    [HttpGet(Name = "GetAllPatients")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PatientResponse>>> GetAll()
    {
        var result = await _patientRepository.GetAllAsync();
        var patients = result.ToList();
        if (!patients.Any()) return NotFound();
        
        return Ok(patients.Select(p => p.ToPatientResponse()));
    }

    [HttpGet("{id}", Name = "GetPatient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientResponse>> Get(string id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null) return NotFound();
       
        return Ok(patient.ToPatientResponse());
    }

    [HttpPost(Name = "CreateWithDocuments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateWithDocumentsAsync([FromForm] PatientDocumentsUploadModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Message = "Invalid input data", Errors = ModelState });
        }

        try
        {
            var patient = JsonConvert.DeserializeObject<Patient>(model.Patient);
            var result = await _patientRepository.CreateWithDocumentsAsync(patient, model.Files);

            if (result != null)
            {
                return Created("Created", result);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Failed to create patient" });
        }
        catch (Exception ex)
        {
            // Logging in the future
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = "An error occurred while creating the patient" });
        }
    }

    [HttpDelete("id")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeletePatient(string id)
    {
        try
        {
            var patient = await _patientRepository.GetByIdAsync(id);

            if (patient == null)
            {
                return NotFound(new { Message = "Patient not found" });
            }

            await _patientRepository.DeleteByIdAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            // Log the exception (ex) here
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while deleting the patient" });
        }
    }
}
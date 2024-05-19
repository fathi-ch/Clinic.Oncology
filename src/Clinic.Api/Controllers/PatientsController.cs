using Clinic.Api.BusinessService;
using Clinic.Core.Contracts;
using Clinic.Core.Dto;
using Clinic.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("/v1/api/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
  

    public PatientsController(IPatientService patientService)
    {
        this._patientService = patientService;


    }

    [HttpGet(Name = "GetAllPatientsAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PatientResponse>>> GetAllAsync()
    {
        var patients = await _patientService.GetAllAsync();
        
        if (!patients.Any()) return Ok(Enumerable.Empty<PatientResponse>()) ;

        return Ok(patients);
    }

    [HttpGet,Route("SearchPatients")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IEnumerable<PatientResponse>> GetAllByNameAsync(string firstName="")
        => (await _patientService.GetAllByNameAsync(firstName));


    [HttpGet("{id}", Name = "GetPatientAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientResponse>> GetAsync(int id)
    {
        var patient = await _patientService.GetAsync(id);
        if (patient == null) return NotFound();

        return Ok(patient);
    }

    [HttpGet("{id}/visits", Name = "GetVisitesByPatientIdAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<VisitResponse>>> GetVisitesByPatientIdAsync(int id)
    {
        var visits = await _patientService.GetVisitesByPatientIdAsync(id);

        if (!visits.Any()) return Ok(Enumerable.Empty<VisitResponse>());

        return Ok(visits);
    }

    [HttpPost(Name = "CreatePatient")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    public async Task<IActionResult> CreateAsync([FromBody] PatientDto patientDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Message = "Invalid input data", Errors = ModelState });
        }

        try
        {
            var patient = await _patientService.CreateAsync(patientDto);

            if (patient != null)
            {
                return Created("Created", patient);
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

    [HttpDelete("{id}", Name = "DeletePatient")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteByIdAsync(int id)
    {
        try
        {
             await _patientService.DeleteByIdAsync(id);

            return Ok();
        }
        catch (Exception ex)
        {
            // Log the exception (ex) here
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = "An error occurred while deleting the patient" });
        }
    }
    
    [HttpPut("{id}", Name = "UpdatePatientAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientResponse>> UpdateAsync(int id, PatientDto patientDto)
    {
        var patient = await _patientService.GetAsync(id);
        if (patient == null) return NotFound();

        var patientInDb = await _patientService.UpdateAsync(id, patientDto);
        
        return Ok(patientInDb);
    }
}
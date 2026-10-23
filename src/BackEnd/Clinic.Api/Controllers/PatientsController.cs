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
    private readonly IPatientReportRepository _patientReportRepo;
    private readonly IDocumentRepository _documentRepository;

    public PatientsController(IPatientService patientService, IPatientReportRepository patientReportRepo,
        IDocumentRepository documentRepository)
    {
        _patientService = patientService;
        _patientReportRepo = patientReportRepo;
        _documentRepository = documentRepository;
    }

    [HttpGet(Name = "GetAllPatientsAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PatientResponse>>> GetAllAsync()
    {
        var patients = await _patientService.GetAllAsync();

        if (!patients.Any()) return Ok(Enumerable.Empty<PatientResponse>());

        return Ok(patients);
    }

    [HttpGet, Route("SearchPatients")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IEnumerable<PatientResponse>> GetAllByNameAsync(string firstName = "")
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

    // Every document of every visit of the patient, for the patient page.
    [HttpGet("{id}/documents", Name = "GetPatientDocumentsAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PatientDocumentResponse>>> GetDocumentsAsync(int id)
    {
        if (await _patientService.GetAsync(id) == null) return NotFound();

        return Ok(await _documentRepository.GetByPatientIdAsync(id));
    }

    [HttpGet("{id}/documents/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportDocumentsAsPdf(int id)
    {

        var report = await _patientReportRepo.GeneratePatientReportPdfAsync(id);

        if (report == null)
        {
            return NotFound(new { Message = "Patient introuvable." });
        }

        return File(report, "application/pdf", $"Dossier_patient_{id}.pdf");

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

        var invalid = Validate(patientDto);
        if (invalid != null) return BadRequest(new { Message = invalid });

        var patientInDb = await _patientService.UpdateAsync(id, patientDto);

        return Ok(patientInDb);
    }

    // The same fields the patient form marks as required, checked here so no client can skip them.
    private static string? Validate(PatientDto patient)
    {
        if (string.IsNullOrWhiteSpace(patient.FirstName)) return "Le nom est obligatoire.";
        if (string.IsNullOrWhiteSpace(patient.LastName)) return "Le prénom est obligatoire.";
        if (string.IsNullOrWhiteSpace(patient.Mobile)) return "Le mobile est obligatoire.";
        if (patient.BirthDate == default || patient.BirthDate.Date > DateTime.Today)
            return "La date de naissance est invalide.";
        return null;
    }
}

using Clinic.Core.Contracts;
using Clinic.Core.Mappers;
using Clinic.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("/v1/api/[controller]")]
public class PatientDocumentsController : ControllerBase
{
    private readonly IPatientDocumentRepository _patientDocument;

    public PatientDocumentsController(IPatientDocumentRepository patientDocument)
    {
        _patientDocument = patientDocument;
    }

    [HttpGet("{id}", Name = "GetPatientDocumentById")]
    public async Task<ActionResult<IEnumerable<PatientDocumentResponse>>> GetPatientDocumentById(string id)
    {
        var patientDocuments = await _patientDocument.GetPatientDocumentByPatientId(id);
        var documents = patientDocuments.ToList();
        if (!documents.Any()) return NotFound();

        return Ok(documents.Select(d => d.ToDocumentResponse()));
    }
}
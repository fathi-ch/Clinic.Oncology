using Clinic.Core.Contracts;
using Clinic.Core.Mappers;
using Clinic.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("/v1/api/patient/{id}/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentRepository _document;

    public DocumentsController(IDocumentRepository document)
    {
        _document = document;
    }

    [HttpGet(Name = "GetPatientDocumentByIdAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PatientWithDocumentsResponse>>> GetPatientDocumentByIdAsync(string id)
    {
        var patientDocuments = await _document.GetPatientDocumentByPatientIdAsync(id);
        
        if (patientDocuments == null) return NotFound();

        return Ok(patientDocuments);
    }
}
using Clinic.Core.Contracts;
using Clinic.Core.Dto;
using Clinic.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("/v1/api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentsController(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    [HttpPost(Name = "CreateDocument")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync([FromForm] PatientDocumentDto patientDocumentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Message = "Invalid input data", Errors = ModelState });
        }

        try
        {
            var documents = await _documentRepository.CreateAsync(patientDocumentDto);

            if (documents != null)
            {
                return Created("Created", documents);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Failed to create Documents" });
        }
        catch (Exception ex)
        {
            // Logging in the future
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = "An error occurred while creating the Documents" });
        }
    }

    [HttpGet(Name = "GetAllAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PatientDocumentResponse>>> GetAllAsync()
    {
        var documents = await _documentRepository.GetAllAsync();

        if (documents.Count() == 0) return Ok(Enumerable.Empty<PatientDocumentResponse>());

        return Ok(documents);
    }

    [HttpGet("{id}", Name = "GetByIdAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientDocumentResponse>> GetByIdAsync(int id)
    {
        var patientDocumentResponse = await _documentRepository.GetByIdAsync(id);
        if (patientDocumentResponse == null) return NotFound();

        return Ok(patientDocumentResponse);
    }

    [HttpDelete("{id}", Name = "DeleteDocument")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteByIdAsync(int id)
    {
        try
        {
            var Document = await _documentRepository.DeleteByIdAsync(id);

            if (Document == null)
            {
                return NotFound(new { Message = "Document not found" });
            }

            return Ok(Document);
        }
        catch (Exception ex)
        {
            // Log the exception (ex) here
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = "An error occurred while deleting the Document" });
        }
    }

    [HttpPut("{id}", Name = "UpdateDocumentAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientDocumentResponse>> UpdateAsync(int id, [FromForm] PatientDocumentDto patientDocumentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); }

        var document = await _documentRepository.GetByIdAsync(id);
        if (document == null) return NotFound();

        var documentInDb = await _documentRepository.UpdateByIdAsync(id, patientDocumentDto);

        return Ok(documentInDb);
    }
}
using Clinic.Core.Contracts;
using Clinic.Core.Dto;
using Clinic.Core.Services;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Previewer;
using System.IO;

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
    public async Task<IActionResult> CreateAsync(PatientDocumentDto patientDocumentDto)
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

    [HttpGet("export/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportDocumentsAsPdf(int id)
    {

        var documentData = await _documentRepository.GetByIdAsync(id);
        if (documentData == null) return NotFound("Document not found.");

        try
        {
            QuestPDF.Settings.License = LicenseType.Community;

            byte[] imageBytes = null;
            if (!string.IsNullOrEmpty(documentData.PatientDocumentsbase64))
            {
                imageBytes = Convert.FromBase64String(documentData.PatientDocumentsbase64);
            }
            
           

           var repport = Document.Create(container =>
            {
                _ = container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);

                    page.Header()

                    .Text("CABINET D'ONCOLOGIE MEDICALE")
                    .SemiBold().FontSize(15).FontColor(Colors.Brown.Darken1).Underline().AlignCenter();



                    page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .PaddingHorizontal(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        //x.Spacing(10);
                        x.Item().Text("DR AMRANE.L");
                        x.Item().Text("ONCOLOGUE MEDICALE");
                        x.Spacing(10);
                        // x.Item().Image(imageBytes, ImageScaling.FitWidth);
                        x.Item().Image(imageBytes).FitHeight();

                    });


                });
            })            
            .GeneratePdf();

            
            return File(repport, "application/pdf", $"Document_{documentData.Name}.pdf");            
        }
        catch (Exception ex)
        {
            // Log and handle general exceptions
            Console.WriteLine("Exception: " + ex.Message);
            Console.WriteLine("Stack Trace: " + ex.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error while generating PDF document." });
        }
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
            return BadRequest(ModelState);
        }

        var document = await _documentRepository.GetByIdAsync(id);
        if (document == null) return NotFound();

        var documentInDb = await _documentRepository.UpdateByIdAsync(id, patientDocumentDto);

        return Ok(documentInDb);
    }

    private Document GeneratePdfDocument(PatientDocumentResponse documentData)
    {
        return null;

        // return Document.Create(container =>
        // {
        //     container.Page(page =>
        //     {
        //         page.Size(PageSizes.A4);
        //         page.Margin(2, Unit.Centimetre);
        //         page.DefaultTextStyle(x => x.FontSize(20));

        //         page.Content()
        //             .Column(column =>
        //             {
        //                 column.Item().Text($"Exported Document: {documentData.Name}").Bold().FontSize(24);
        //                 column.Item().Text($"Visit ID: {documentData.VisitId}");
        //                 column.Item().Text($"Document Type: {documentData.DocumentType}");

        //                 if (!string.IsNullOrEmpty(documentData.PatientDocumentsbase64))
        //                 {
        //                     byte[] documentBytes = Convert.FromBase64String(documentData.PatientDocumentsbase64);
        //                     using (var documentStream = new MemoryStream(documentBytes))
        //                     {
        //                         var image = ImageSource.FromStream(documentStream);
        //                         column.Item().Image(image);
        //                     }
        //                 }
        //             });
        //     });
        // });
    }
}
using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Dto;

public class PatientDocumentDto
{
    public IEnumerable<IFormFile?> Files { get; set; } = new List<IFormFile?>(); // To bypass the endpoint validations
    public int VisitId { get; set; }
    public string? DocumentType { get; set; }
}
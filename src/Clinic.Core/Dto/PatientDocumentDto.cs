using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Dto;

public class PatientDocumentDto
{
    public string File64 { get; set; } // To bypass the endpoint validations
    public int VisitId { get; set; }
    public string FileName { get; set; }
}
using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Dto;

public class VisitDto
{
    public int PatientId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public float Price { get; set; }
    public string Description { get; set; }
    IEnumerable<IFormFile?> Documents { get; set; }
}
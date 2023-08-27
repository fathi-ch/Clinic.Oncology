namespace Clinic.Core.Contracts;

public class VisitResponse
{
    public int Id { get; set; }
    public int PatientId { get; set; } 
    public PatientResponse Patient { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public float Price { get; set; }
    public string Description { get; set; }
    public string VisitType { get; set; }
    public string Status { get; set; }
    public IEnumerable<PatientDocumentResponse> Documents { get; set; } 
}
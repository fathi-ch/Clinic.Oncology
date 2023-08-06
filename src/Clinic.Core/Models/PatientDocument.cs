namespace Clinic.Core.Models;

public class PatientDocument
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Path { get; set; }
}
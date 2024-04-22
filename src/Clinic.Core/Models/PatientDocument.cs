namespace Clinic.Core.Models;

public class PatientDocument
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? DocumentType { get; set; }
    public int VisitId { get; set; }
    //public Visit Visit { get; set; }
}
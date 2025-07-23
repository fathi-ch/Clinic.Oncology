namespace Clinic.Core.Contracts;

public class PatientDocumentResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int VisitId { get; set; }
    public string DocumentType { get; set; }
    public string PatientDocumentsbase64 { get; set; }
}
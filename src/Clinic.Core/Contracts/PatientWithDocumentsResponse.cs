using Clinic.Core.Models;

namespace Clinic.Core.Contracts;

public class PatientWithDocumentsResponse
{
    public PatientResponse Patient { get; set; }
    public List<PatientDocumentResponse> Documents { get; set; }
}
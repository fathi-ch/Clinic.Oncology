using Clinic.Core.Models;

namespace Clinic.Core.Contracts;

public class PatientWithDocumentsResponse
{
    public Patient Patient { get; set; }
    public List<PatientDocument> Documents { get; set; }
}
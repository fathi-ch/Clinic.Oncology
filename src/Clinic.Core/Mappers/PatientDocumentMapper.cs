using Clinic.Core.Contracts;
using Clinic.Core.Models;

namespace Clinic.Core.Mappers;

public static class PatientDocumentMapper
{
    public static PatientDocumentsResponse ToDocumentResponse(this PatientDocument? patientDocument)
    {
        var convertedList = new List<string>();
        var imageByte = File.ReadAllBytes(patientDocument.Path);

        return new PatientDocumentsResponse
        {
            Path = patientDocument.Path,
            PatientDocumentbase64 = Convert.ToBase64String(imageByte)
        };
    }
}
using Clinic.Core.Contracts;
using Clinic.Core.Models;

namespace Clinic.Core.Mappers;

public static class PatientDocumentMapper
{
    public static PatientDocumentResponse ToDocumentResponse(this PatientDocument patientDocument)
    {
        List<string> convertedList = new List<string>();
        byte[] imageByte = System.IO.File.ReadAllBytes(patientDocument.Path);
        
        return new PatientDocumentResponse()
        {
            Path = patientDocument.Path,
            PatientDocumentbase64 = Convert.ToBase64String(imageByte)
        };
    }
}
using System.Diagnostics;
using Clinic.Core.Contracts;
using Clinic.Core.Helpers;
using Clinic.Core.Models;

namespace Clinic.Core.Mappers;

public static class PatientDocumentMapper
{
    public static PatientDocumentResponse ToDocumentResponse(this PatientDocument? patientDocument)
    {
        //Code refactor needed to provide the path properly 
        Guard.IsNotNull(patientDocument.Name, nameof(patientDocument.Name));
        var imageByte = File.ReadAllBytes(patientDocument.Name);
        
        return new PatientDocumentResponse
        {
            
            Path = patientDocument.Name,
            PatientDocumentbase64 = Convert.ToBase64String(imageByte)
        };
    }
}
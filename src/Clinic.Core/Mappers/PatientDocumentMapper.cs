using System.Diagnostics;
using Clinic.Core.Configurations;
using Clinic.Core.Contracts;
using Clinic.Core.Dto;
using Clinic.Core.Helpers;
using Clinic.Core.Models;

namespace Clinic.Core.Mappers;

public static class PatientDocumentMapper
{
    public static PatientDocumentResponse ToDocumentResponse(this PatientDocument patientDocument, string documentPath)
    {
        var path = Path.Combine(documentPath, patientDocument.Name);
        var imageByte = File.ReadAllBytes(path);
        
        return new PatientDocumentResponse()
        {
            Id = patientDocument.Id,
            VisitId = patientDocument.VisitId,
            Name = patientDocument.Name,
            DocumentType = patientDocument.DocumentType,
            PatientDocumentsbase64 = Convert.ToBase64String(imageByte)

        };
    }
    
    public static PatientDocumentResponse ToDocumentResponse(this PatientDocumentDto patientDocumentDto, int id, string name, string documentPath)
    {
        var path = Path.Combine(documentPath, name);
        var imageByte = File.ReadAllBytes(path);
       
        return new PatientDocumentResponse()
        {
            Id = id,
            VisitId = patientDocumentDto.VisitId,
            Name = name,
            DocumentType = patientDocumentDto.DocumentType,
            PatientDocumentsbase64 = Convert.ToBase64String(imageByte)
        };
    }
    
    //Comming back to this method to reduce code duplication
    private static string DocumentImageConverter(string name, string documentPath)
    {
        var path = Path.Combine(documentPath, name);
        var imageByte = File.ReadAllBytes(path);
        return Convert.ToBase64String(imageByte);
    }
    
    
}
﻿using System.Diagnostics;
using Clinic.Core.Contracts;
using Clinic.Core.Helpers;
using Clinic.Core.Models;

namespace Clinic.Core.Mappers;

public static class PatientDocumentMapper
{
    public static PatientDocumentResponse ToDocumentResponse(this PatientDocument? patientDocument)
    {
        Guard.IsNotNull(patientDocument.Path, nameof(patientDocument.Path));
        var imageByte = File.ReadAllBytes(patientDocument.Path);

        return new PatientDocumentResponse
        {
            Path = patientDocument.Path,
            PatientDocumentbase64 = Convert.ToBase64String(imageByte)
        };
    }
}
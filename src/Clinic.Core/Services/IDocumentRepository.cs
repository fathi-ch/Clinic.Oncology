﻿using Clinic.Core.Contracts;
using Clinic.Core.Dto;

namespace Clinic.Core.Services;

public interface IDocumentRepository
{
    Task<IEnumerable<PatientDocumentResponse>> CreateAsync(PatientDocumentDto patientDocumentDto);
    Task<IEnumerable<PatientDocumentResponse>> GetAllAsync();
    Task<PatientDocumentResponse?> GetByIdAsync(int id);
    Task<IEnumerable<PatientDocumentResponse>> GetByVisitIdAsync(int visitId);
    Task<PatientDocumentResponse> DeleteByIdAsync(int id);
    Task<IEnumerable<PatientDocumentResponse>> DeleteByVisitIdAsync(int visitId);
}
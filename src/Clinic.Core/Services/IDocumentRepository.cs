using Clinic.Core.Contracts;
using Clinic.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Services;

public interface IDocumentRepository
{
    Task<bool> CreatePatientDocumentsAsync(IEnumerable<IFormFile> files, string id);
    Task<PatientWithDocumentsResponse> GetPatientDocumentByPatientIdAsync(string id);
    Task<bool> DeletePatientDocumentsAsync(string id);
}
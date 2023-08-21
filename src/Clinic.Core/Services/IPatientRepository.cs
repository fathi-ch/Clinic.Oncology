using Clinic.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Services;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(string id);
    Task<bool> CreateWithDocumentsAsync(Patient patient, IEnumerable<IFormFile> files);
    Task<bool> DeleteByIdAsync(string id);
}
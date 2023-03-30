using Clinic.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Repositories;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllAsync();
    
    Task<IEnumerable<Patient>> GetAllWithDocumentsAsync();
    
    Task<Patient?> GetByIdWithDocumentsAsync(Guid id);
    Task<Patient?> GetByIdAsync(Guid id);
    Task<bool> CreateWithDocumentsAsync(Patient patient, IEnumerable<IFormFile> files);
    Task<bool> DeleteByIdAsync(Guid id);
}
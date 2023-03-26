using Clinic.Core.Models;

namespace Clinic.Core.Repositories;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllAsync();
    
    Task<IEnumerable<Patient>> GetAllWithDocumentsAsync();
    
    Task<Patient?> GetByIdWithDocumentsAsync(Guid id);
    Task<Patient?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(Patient patient);
    Task<bool> DeleteByIdAsync(Guid id);
}
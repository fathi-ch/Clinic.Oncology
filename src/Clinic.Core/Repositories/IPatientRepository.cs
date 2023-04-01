using Clinic.Core.Models;
using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Repositories;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(string id);
   
    // This will be move to document to separate responsibility from Patient to PatientDocument
    // Refactor the IFromFile to turn this method more generic, remove tight coupling 
    Task<bool> CreateWithDocumentsAsync(Patient patient, IEnumerable<IFormFile> files);
    Task<bool> DeleteByIdAsync(Guid id);
}
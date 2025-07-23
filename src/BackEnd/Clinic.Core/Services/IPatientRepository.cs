using Clinic.Core.Contracts;
using Clinic.Core.Dto;

namespace Clinic.Core.Services;

public interface IPatientRepository
{
    Task<IEnumerable<PatientResponse>> GetAllAsync();
    Task<PatientResponse?> GetByIdAsync(int id);
    Task<PatientResponse> CreateAsync(PatientDto patientDto);
    Task<PatientResponse> DeleteByIdAsync(int id);
    Task<IEnumerable<PatientResponse>> GetAllByNameAsync(string name);
    Task<PatientResponse> UpdateByIdAsync(int id, PatientDto patientDto);
}
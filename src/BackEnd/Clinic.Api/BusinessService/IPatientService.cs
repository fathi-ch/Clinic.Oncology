using Clinic.Core.Contracts;
using Clinic.Core.Dto;

namespace Clinic.Api.BusinessService
{
    public interface IPatientService
    {
        Task<IEnumerable<PatientResponse>> GetAllAsync();

        Task<IEnumerable<PatientResponse>> GetAllByNameAsync(string firstName);

        Task<PatientResponse> GetAsync(int id);

        Task<IEnumerable<VisitResponse>> GetByDateAsync(int id);

        Task<PatientResponse> CreateAsync(PatientDto patientDto);

        Task DeleteByIdAsync(int id);
        Task<PatientResponse> UpdateAsync(int id, PatientDto patientDto);

        Task<IEnumerable<VisitResponse>> GetVisitesByPatientIdAsync(int id);


    }
}

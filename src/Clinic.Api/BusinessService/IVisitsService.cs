using Clinic.Core.Contracts;
using Clinic.Core.Dto;

namespace Clinic.Api.BusinessService
{
    public interface IVisitsService
    {

        Task<IEnumerable<VisitResponse>> GetAllAsync();
        Task<IEnumerable<VisitResponse>> GetBydDateAsync(DateTime fromDate, DateTime toDate);
        Task<VisitResponse> GetByIdAsync(int id);
        Task<VisitResponse> CreateAsync(VisitDto VisitDto);
        Task<IEnumerable<PatientDocumentResponse>> GetDocumentsByVisitIdAsync(int visitid);
        Task DeleteByIdAsync(int id);
        Task<VisitResponse> UpdateByIdAsync(int id, VisitDto visitDto);


    }
}

using Clinic.Core.Contracts;
using Clinic.Core.Dto;

namespace Clinic.Core.Services;

public interface IVisitRepository
{
   Task<VisitResponse> CreateAsync(VisitDto visitDto);
   Task<IEnumerable<VisitResponse>> GetAllAsync();
   Task<VisitResponse> GetByIdAsync(int id);
   Task<VisitResponse> DeleteByIdAsync(int id);
   
}
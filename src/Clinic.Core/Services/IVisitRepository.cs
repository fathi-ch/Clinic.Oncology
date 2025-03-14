﻿using Clinic.Core.Contracts;
using Clinic.Core.Dto;

namespace Clinic.Core.Services;

public interface IVisitRepository
{
    Task<VisitResponse> CreateAsync(VisitDto visitDto);
    Task<IEnumerable<VisitResponse>> GetAllAsync();
    Task<VisitResponse> GetByIdAsync(int id);
    Task<VisitResponse> DeleteByIdAsync(int id);
    // Task UpdateAsync(VisitDto visitDto);
    Task<IEnumerable<VisitResponse>> GetBydDateAsync(DateTime fromDate, DateTime toDate);
    Task<VisitResponse> UpdateByIdAsync(int id, VisitDto visitDto);


}
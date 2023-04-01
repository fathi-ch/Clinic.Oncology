using Clinic.Core.Contracts;
using Clinic.Core.Mappers;
using Clinic.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PatientController : ControllerBase
{
    private readonly IPatientRepository _patient;
    
    public PatientController(IPatientRepository patient)
    {
        _patient = patient;
    }
    
    [HttpGet(Name = "GetAllPatients")]
    public async Task<IEnumerable<PatientResponse>> GetAll()
    {
        var patient = await _patient.GetAllAsync();
        return patient.ToList().Select(p => p.ToPatientResponse());
    }
}
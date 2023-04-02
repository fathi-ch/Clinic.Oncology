using Clinic.Core.Contracts;
using Clinic.Core.Helpers;
using Clinic.Core.Models;

namespace Clinic.Core.Mappers;

public static class PatientMapper
{
    public static PatientResponse ToPatientResponse(this Patient patient)
    {
        return new PatientResponse
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            BirthDate = patient.BirthDate,
            NextAppointment = patient.NextAppointment,
            Age = patient.BirthDate.GetCurrentAge()
        };
    }
}
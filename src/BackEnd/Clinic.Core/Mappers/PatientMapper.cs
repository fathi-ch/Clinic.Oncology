using System.Globalization;
using Clinic.Core.Contracts;
using Clinic.Core.Dto;
using Clinic.Core.Helpers;
using Clinic.Core.Models;

namespace Clinic.Core.Mappers;

public static class PatientMapper
{
    public static PatientResponse ToPatientResponse(this Patient patient)
    {
        if (patient == null)
        {
            return null;
        }
        
        return new PatientResponse
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            BirthDate = patient.BirthDate,
            Age = patient.BirthDate.GetCurrentAge(),
            NextAppointment = patient.NextAppointment,
            Gender = patient.Gender,
          
            Mobile = patient.Mobile,
            SocialSecurityNumber = patient.SocialSecurityNumber,
            Referral = patient.Referral,
            Email=patient.Email,
            
        };
    }

    public static IEnumerable<PatientResponse> ToPatientResponse(this IEnumerable<Patient> patients)
    {
        if (patients == null)
        {
            return Enumerable.Empty<PatientResponse>();
        }
        List<PatientResponse> pateintsRespoins = new List<PatientResponse>();
        foreach (Patient patient in patients)
        {
            pateintsRespoins.Add(new PatientResponse
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                BirthDate = patient.BirthDate,
                Age = patient.BirthDate.GetCurrentAge(),
                NextAppointment = patient.NextAppointment,
                Gender = patient.Gender,

                Mobile = patient.Mobile,
                SocialSecurityNumber = patient.SocialSecurityNumber,
                Referral = patient.Referral,
                Email = patient.Email,

            });
        }
        return pateintsRespoins;
    }

    public static Patient ToPatient(this PatientDto patientDto)
    {
        if (patientDto == null)
        {
            return null;
        }
        
        return new Patient()
        {
            FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(patientDto.FirstName ?? string.Empty),
            LastName = patientDto.LastName?.ToUpper(),
            BirthDate = patientDto.BirthDate,
            NextAppointment = patientDto.NextAppointment,
            Gender = patientDto.Gender,
            Mobile = patientDto.Mobile,
            SocialSecurityNumber = patientDto.SocialSecurityNumber,
            Referral = patientDto.Referral,
            Email = patientDto.Email,
        };
    }
    
    public static PatientResponse ToPatientResponse(this PatientDto patientDto, int id)
    {
        if (patientDto == null)
        {
            return null;
        }
        
        return new PatientResponse()
        {
            Id = id,
            FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(patientDto.FirstName ?? string.Empty),
            LastName = patientDto.LastName?.ToUpper(),
            BirthDate = patientDto.BirthDate,
            Age = patientDto.BirthDate.GetCurrentAge(),
            NextAppointment = patientDto.NextAppointment,
            Gender = patientDto.Gender,
            Mobile = patientDto.Mobile,
            SocialSecurityNumber = patientDto.SocialSecurityNumber,
            Referral = patientDto.Referral,
            Email = patientDto.Email
        };
    }

    public static PatientDto ToPatientDto(this PatientResponse patientResponse, int id)
    {
        if (patientResponse == null)
        {
            return null;
        }

        return new PatientDto()
        {
            id = id,
            FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(patientResponse.FirstName ?? string.Empty),
            LastName = patientResponse.LastName?.ToUpper(),
            BirthDate = patientResponse.BirthDate,
            Age = patientResponse.BirthDate.GetCurrentAge(),
            NextAppointment = patientResponse.NextAppointment,
            Gender = patientResponse.Gender,
            Mobile = patientResponse.Mobile,
            SocialSecurityNumber = patientResponse.SocialSecurityNumber,
            Referral = patientResponse.Referral
        };
    }
}
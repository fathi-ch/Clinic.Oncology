using Clinic.Core.Contracts;
using Clinic.Core.Dto;
using Clinic.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Clinic.Core.Mappers;

public static class VisitMapper
{
    // Visit times arrive from the UI as UTC and SQLite stores them without a zone.
    // Mark them UTC again so the JSON carries a "Z" and browsers convert them to local time.
    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    public static VisitResponse ToVisitResponse(this VisitDto visitDto)
    {
        if (visitDto == null)
        {
            return null;
        }

        return new VisitResponse()
        {

            Id = visitDto.Id,
            PatientId = visitDto.PatientId,
            StartTime = visitDto.StartTime,
            EndTime = visitDto.EndTime,
            Price = visitDto.Price,
            Description = visitDto.Description,
            VisitType = visitDto.VisitType,
            Status = visitDto.Status,
            Weight = visitDto.Weight,
            Height = visitDto.Height,
            BillingType = visitDto.BillingType
        };
    }

    public static VisitResponse ToVisitResponse(this VisitDto visitDto,int id)
    {
        if (visitDto == null)
        {
            return null;
        }

        return new VisitResponse()
        {

            Id = id,
            PatientId = visitDto.PatientId,
            StartTime = visitDto.StartTime,
            EndTime = visitDto.EndTime,
            Price = visitDto.Price,
            Description = visitDto.Description,
            VisitType = visitDto.VisitType,
            Status = visitDto.Status,
            Weight = visitDto.Weight,
            Height = visitDto.Height,
            BillingType = visitDto.BillingType
        };
    }

    public static VisitResponse ToVisitResponse(this Visit visit)
    {
        if (visit == null)
        {
            return null;
        }

        return new VisitResponse()
        {
            Id = visit.Id,
            PatientId = visit.PatientId,
            Patient = visit.Patient.ToPatientResponse(),
            StartTime = AsUtc(visit.StartTime),
            EndTime = AsUtc(visit.EndTime),
            Price = visit.Price,
            Description = visit.Description,
            VisitType = visit.VisitType,
            Status = visit.Status,
            Weight = visit.Weight,
            Height = visit.Height,
            BillingType = visit.BillingType
        };
    }
    public static IEnumerable<VisitResponse> ToVisitResponse(this IEnumerable<Visit> visits)
    {
        if (visits == null)
        {
            return Enumerable.Empty<VisitResponse>();
        }

        List<VisitResponse> result = new List<VisitResponse>();

        foreach (Visit visit in visits)
        {
            result.Add(new VisitResponse()
            {
                Id = visit.Id,
                PatientId = visit.PatientId,
                Patient = visit.Patient.ToPatientResponse(),
                StartTime = AsUtc(visit.StartTime),
                EndTime = AsUtc(visit.EndTime),
                Price = visit.Price,
                Description = visit.Description,
                VisitType = visit.VisitType,
                Status = visit.Status,
                Weight = visit.Weight,
                Height = visit.Height,
            BillingType = visit.BillingType
            });
        }
        return result;
    }

    public static VisitResponse ToVisitResponse(this Visit visit, PatientResponse patient)
    {
        if (visit == null)
        {
            return null;
        }

        return new VisitResponse()
        {
            Id = visit.Id,
            PatientId = visit.PatientId,
            Patient = patient,
            StartTime = AsUtc(visit.StartTime),
            EndTime = AsUtc(visit.EndTime),
            Price = visit.Price,
            Description = visit.Description,
            VisitType = visit.VisitType,
            Status = visit.Status,
            Weight = visit.Weight,
            Height = visit.Height,
            BillingType = visit.BillingType
        };
    }
}
using Clinic.Core.Contracts;
using Clinic.Core.Dto;
using Clinic.Core.Models;
using System.Linq;

namespace Clinic.Core.Mappers;

public static class VisitMapper
{
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
            Status = visitDto.Status
        };
    }

    public static VisitResponse ToVisitResponse(this Visit visit, IEnumerable<PatientDocumentResponse> documents)
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
            StartTime = visit.StartTime,
            EndTime = visit.EndTime,
            Price = visit.Price,
            Description = visit.Description,
            VisitType = visit.VisitType,
            Status = visit.Status,
            Documents = documents
        };
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
            StartTime = visit.StartTime,
            EndTime = visit.EndTime,
            Price = visit.Price,
            Description = visit.Description,
            VisitType = visit.VisitType,
            Status = visit.Status,
            Documents = null
        };
    }
}
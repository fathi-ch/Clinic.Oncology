using Clinic.Core.Models;

namespace Clinic.Core.Repositories;

public interface IPatientDocumentRepository
{
    Task<IEnumerable<PatientDocument?>> GetPatientDocumentByPatientId(string id);
}
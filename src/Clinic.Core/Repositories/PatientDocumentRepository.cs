using Clinic.Core.Data;
using Clinic.Core.Models;
using Dapper;

namespace Clinic.Core.Repositories;

public class PatientDocumentRepository : IPatientDocumentRepository
{
    private readonly ISqliteDbConnectionFactory _connection;

    public PatientDocumentRepository(ISqliteDbConnectionFactory connection)
    {
        _connection = connection;
    }
    public async Task<IEnumerable<PatientDocument?>> GetPatientDocumentByPatientId(string id)
    {
        using var connection = await _connection.CreateDbConnectionAsync();
        return  await connection.QueryAsync<PatientDocument>(
            "SELECT * " +
            "FROM Documents d " +
            "WHERE PatientId = @PatientId;",
            new { PatientId = Guid.Parse(id) });
        
    }
}
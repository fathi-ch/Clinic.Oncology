using System.Text;
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

        var sb = new StringBuilder();
        sb.Append("SELECT * ");
        sb.Append("FROM Documents d ");
        sb.Append("WHERE PatientId = @PatientId;");

        var query = sb.ToString();
        return await connection.QueryAsync<PatientDocument>(
            query,
            new { PatientId = Guid.Parse(id) });
    }
}
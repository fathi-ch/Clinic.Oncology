using Clinic.Core.Data;
using Clinic.Core.Models;
using Dapper;

namespace Clinic.Core.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ISqliteDbConnectionFactory _connectionFactory;
    public PatientRepository(ISqliteDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        return await connection.QueryAsync<Patient>("select * from Patients");
    }

    public Task<IEnumerable<Patient>> GetAllWithDocumentsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Patient?> GetByIdWithDocumentsAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Patient?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CreateAsync(Patient patient)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
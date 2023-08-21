using System.Globalization;
using System.Text;
using Clinic.Core.Data;
using Clinic.Core.Models;
using Dapper;
using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Services;

public class PatientRepository : IPatientRepository
{
    private readonly ISqliteDbConnectionFactory _connectionFactory;
    private readonly IDocumentRepository _documentRepository;

    public PatientRepository(ISqliteDbConnectionFactory connectionFactory, IDocumentRepository documentRepository)
    {
        _connectionFactory = connectionFactory;
        _documentRepository = documentRepository;
    }

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();

        var sb = new StringBuilder();
        sb.Append("SELECT * ");
        sb.Append("FROM Patients p ");

        var query = sb.ToString();
        return await connection.QueryAsync<Patient>(query);
    }

    public async Task<bool> CreateWithDocumentsAsync(Patient patient, IEnumerable<IFormFile> files)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO ");
            sb.Append("Patients (Id, FirstName, LastName, BirthDate, NextAppointment) ");
            sb.Append("VALUES (@Id, @FirstName, @LastName, @BirthDate, @NextAppointment); ");

            var query = sb.ToString();

            //Possible to override ToString to contain this logic -Refactor-
            patient.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(patient.FirstName ?? string.Empty);
            patient.LastName = patient.LastName?.ToUpper();

            patient.Id = Guid.NewGuid();
            var result = await connection.ExecuteAsync(query,
                new { patient.Id, patient.FirstName, patient.LastName, patient.BirthDate, patient.NextAppointment });

            transaction.Commit();
            await _documentRepository.CreatePatientDocumentsAsync(files, patient.Id.ToString());
            return result > 0;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<Patient?> GetByIdAsync(string id)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        var sb = new StringBuilder();
        sb.Append("SELECT * ");
        sb.Append("FROM Patients p ");
        sb.Append("WHERE p.Id = @PatientId;");

        var query = sb.ToString();
        return await connection.QueryFirstOrDefaultAsync<Patient>(
            query,
            new { PatientId = Guid.Parse(id) });
    }

    public async Task<bool> DeleteByIdAsync(string id)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();

        await _documentRepository.DeletePatientDocumentsAsync(id);

        var sb = new StringBuilder();
        sb.Append("DELETE ");
        sb.Append("FROM Patients ");
        sb.Append("WHERE Id = @PatientId;");

        var query = sb.ToString();
        var affectedPatientRows = await connection.ExecuteAsync(
            query,
            new { PatientId = id });

        return affectedPatientRows > 0;
    }
}
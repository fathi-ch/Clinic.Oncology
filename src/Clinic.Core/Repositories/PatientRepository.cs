using System.Globalization;
using Clinic.Core.Data;
using Clinic.Core.Models;
using Dapper;
using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ISqliteDbConnectionFactory _connectionFactory;
    private readonly IFileRepository _file;

    public PatientRepository(ISqliteDbConnectionFactory connectionFactory, IFileRepository file)
    {
        _connectionFactory = connectionFactory;
        _file = file;
    }

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        return await connection.QueryAsync<Patient>("select * from Patients");
    }

    public async Task<bool> CreateWithDocumentsAsync(Patient patient, IEnumerable<IFormFile> files)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string query = "INSERT INTO Patients (Id, FirstName, LastName, BirthDate, NextAppointment)" +
                                 " VALUES (@Id, @FirstName, @LastName, @BirthDate, @NextAppointment);";
            patient.Id = Guid.NewGuid();

            //Possible to ovrride ToString to contain this logic
            patient.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(patient.FirstName);
            patient.LastName = patient.LastName.ToUpper();
            
            var result = await connection.ExecuteAsync(query,
                new { patient.Id, patient.FirstName, patient.LastName, patient.BirthDate, patient.NextAppointment });

            transaction.Commit();
            await _file.SaveFilesAsync(files, patient.Id);
            return result > 0;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
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


    public Task<bool> DeleteByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
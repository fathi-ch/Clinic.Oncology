using Clinic.Core.Configurations;
using Clinic.Core.Data;
using Clinic.Core.Helpers;
using Clinic.Core.Models;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Clinic.Core.Repositories;

public class FileRepository : IFileRepository
{
    private readonly ISqliteDbConnectionFactory _connectionFactory;
    private readonly DatabaseConfigurations _dbSettings;

    public FileRepository(ISqliteDbConnectionFactory connectionFactory, DatabaseConfigurations dbConfigs)
    {
        _connectionFactory = connectionFactory;
        _dbSettings = dbConfigs;
    }


    public async Task<bool> SaveFilesAsync(IEnumerable<IFormFile> files, Guid id)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var result = 0;
            if (files.Any())
            {
                const string queryLastPatient = "SELECT * FROM Patients WHERE Id = @Id";
                const string documentQuery =
                    "INSERT INTO Documents (Id, PatientId, Path) VALUES (@Id, @PatientId, @Path)";

                var lastPatient =
                    await connection.QuerySingleOrDefaultAsync<Patient>(queryLastPatient, new { Id = id });

                foreach (var file in files)
                {
                    var docId = Guid.NewGuid();
                    var originalFileExtension = Path.GetExtension(file.FileName);
                    var currentTimeMs = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString();
                    string[] stringsToJoin =
                    {
                        lastPatient.FirstName, docId.ToString(), DateTime.Now.ToString("yyyyMMdd_HHmmss"), currentTimeMs
                    };

                    var fileName = string.Join("_", stringsToJoin);
                    var documentsPath = Path.Combine(_dbSettings.GetFullDocumentsPath(), fileName + originalFileExtension);

                    await using (var fileStream = new FileStream(documentsPath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    result = await connection.ExecuteAsync(documentQuery,
                        new { Id = docId, PatientId = lastPatient.Id, Path = documentsPath });
                }
            }

            transaction.Commit();
            return result > 0;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}
using Clinic.Core.Data;
using Clinic.Core.Models;
using Dapper;
using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Repositories;

public class FileRepository : IFileRepository
{
    private readonly ISqliteDbConnectionFactory _connectionFactory;

    public FileRepository(ISqliteDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
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
                    //Default path to move to appsettings later on
                    var pathPic = Path.Combine(@"D:\Code\Data\Documents\", fileName + originalFileExtension);

                    await using (var fileStream = new FileStream(pathPic, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    result = await connection.ExecuteAsync(documentQuery,
                        new { Id = docId, PatientId = lastPatient.Id, Path = pathPic });
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
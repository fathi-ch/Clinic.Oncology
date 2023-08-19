using System.Text;
using Clinic.Core.Configurations;
using Clinic.Core.Contracts;
using Clinic.Core.Data;
using Clinic.Core.Mappers;
using Clinic.Core.Models;
using Dapper;
using Microsoft.AspNetCore.Http;


namespace Clinic.Core.Services;

public class DocumentRepository : IDocumentRepository
{
    private readonly ISqliteDbConnectionFactory _connectionFactory;
    private readonly IFileRepository _fileRepository;
    private readonly DatabaseConfigurations _dbConfigs;

    public DocumentRepository(ISqliteDbConnectionFactory connectionFactory, IFileRepository fileRepository,
        DatabaseConfigurations dbConfigs)
    {
        _connectionFactory = connectionFactory;
        _fileRepository = fileRepository;
        _dbConfigs = dbConfigs;
    }

    public async Task<bool> CreatePatientDocumentsAsync(IEnumerable<IFormFile> files, string id)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var result = 0;
            if (files.Any())
            {
                const string lastPatientQuery = "SELECT * FROM Patients WHERE Id = @Id";
                const string documentQuery =
                    "INSERT INTO Documents (Id, PatientId, Path) VALUES (@Id, @PatientId, @Path)";

                var lastPatient =
                    await connection.QuerySingleOrDefaultAsync<Patient>(lastPatientQuery, new { Id = id });

                foreach (var file in files)
                {
                    var newDocumentId = Guid.NewGuid();
                    var originalFileExtension = Path.GetExtension(file.FileName);
                    var documentsPath = GenerateDocumentPath(lastPatient, newDocumentId, originalFileExtension);

                    await _fileRepository.SaveFilesAsync(file, documentsPath);
                    result = await connection.ExecuteAsync(documentQuery,
                        new { Id = newDocumentId, PatientId = lastPatient.Id, Path = documentsPath });
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

    public async Task<PatientWithDocumentsResponse> GetPatientDocumentByPatientIdAsync(string id)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        var patientDictionary = new Dictionary<Guid, PatientWithDocumentsResponse>();

        await connection.QueryAsync<Patient, PatientDocument, PatientWithDocumentsResponse>(
            PatientWithDocumentsQuery,
            MapPatientWithDocuments(patientDictionary),
            param: new { PatientId = Guid.Parse(id) },
            splitOn: SplitOnIdColumnName
        );

        return patientDictionary.Values.FirstOrDefault() ?? throw new InvalidOperationException();
    }

    public async Task<bool> DeletePatientDocumentsAsync(string id)
    {
        var patientDocuments = await GetPatientDocumentByPatientIdAsync(id);
        if (patientDocuments?.Patient == null || !patientDocuments.Documents.Any())
        {
            return false;
        }

        var filesDeleted = await DeleteFilesFromDiskAsync(patientDocuments.Documents);
        var dbDeleted = await DeletePatientDocumentsFromDbAsync(id);

        return filesDeleted && dbDeleted;
    }

    private async Task<bool> DeleteFilesFromDiskAsync(IEnumerable<PatientDocumentResponse> patientDocuments)
    {
        var deleteFileTasks = patientDocuments
            .Where(document => document?.Path != null)
            .Select(document => _fileRepository.DeleteFilesAsync(document.Path))
            .ToList();

        await Task.WhenAll(deleteFileTasks);

        return true;
    }

    private string GenerateDocumentPath(Patient lastPatient, Guid newDocumentId, string originalFileExtension)
    {
        var currentTimeMs = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString();
        var timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{lastPatient.FirstName}_{newDocumentId}_{timeStamp}_{currentTimeMs}";

        return Path.Combine(_dbConfigs.GetFullDocumentsPath(), fileName + originalFileExtension);
    }

    private async Task<bool> DeletePatientDocumentsFromDbAsync(string id)
    {
        try
        {
            using var connection = await _connectionFactory.CreateDbConnectionAsync();
            var sb = new StringBuilder();
            sb.Append("DELETE ");
            sb.Append("FROM Documents ");
            sb.Append("WHERE PatientId = @PatientId;");

            var deletePatientQuery = sb.ToString();
            var affectedRows = await connection.ExecuteAsync(deletePatientQuery, new { PatientId = Guid.Parse(id) });
            return affectedRows > 0;
        }
        catch (Exception e)
        {
            // Log the exception
            return false;
        }
    }

    private static Func<Patient, PatientDocument, PatientWithDocumentsResponse> MapPatientWithDocuments(
        Dictionary<Guid, PatientWithDocumentsResponse> patientDocuments)
    {
        return (patient, document) =>
        {
            if (!patientDocuments.TryGetValue(patient.Id, out var patientEntry))
            {
                patientEntry = new PatientWithDocumentsResponse
                {
                    Patient = patient.ToPatientResponse(),
                    Documents = new List<PatientDocumentResponse>()
                };
                patientDocuments.Add(patient.Id, patientEntry);
            }

            if (document != null)
            {
                patientEntry.Documents.Add(document.ToDocumentResponse());
            }

            return patientEntry;
        };
    }

    private const string SplitOnIdColumnName = "Id";

    private const string PatientWithDocumentsQuery =
        @"SELECT p.*, d.* FROM Patients p LEFT JOIN Documents d ON p.Id = d.PatientId WHERE p.Id = @PatientId;";
}
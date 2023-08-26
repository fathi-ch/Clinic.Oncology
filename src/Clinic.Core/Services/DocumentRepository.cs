using System.Dynamic;
using System.Text;
using Clinic.Core.Configurations;
using Clinic.Core.Contracts;
using Clinic.Core.Data;
using Clinic.Core.Dto;
using Clinic.Core.Mappers;
using Clinic.Core.Models;
using Dapper;


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

    public async Task<IEnumerable<PatientDocumentResponse>> CreateAsync(PatientDocumentDto patientDocumentDto)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var newId = 0;
            var documentsResponse = new List<PatientDocumentResponse>();
            if (patientDocumentDto.Files.Count() > 0)
            {
                var sb = new StringBuilder();
                sb.Append("INSERT INTO Documents (VisitId, Name, DocumentType) ");
                sb.Append("VALUES (@VisitId, @Name, @DocumentType); ");
                sb.Append("SELECT last_insert_rowid();");
                var query = sb.ToString();


                foreach (var file in patientDocumentDto.Files)
                {
                    var originalFileExtension = Path.GetExtension(file.FileName);
                    var documentsFullName = GenerateDocumentPath(patientDocumentDto.VisitId, originalFileExtension);

                    await _fileRepository.SaveFilesAsync(file, documentsFullName);

                    newId = await connection.ExecuteScalarAsync<int>(query,
                        new
                        {
                            VisitId = patientDocumentDto.VisitId, Name = documentsFullName,
                            DocumentType = patientDocumentDto.DocumentType
                        });

                    var documentResponse = patientDocumentDto.ToDocumentResponse(newId, documentsFullName,
                        _dbConfigs.GetFullSaveFolderPathForDocuments(documentsFullName));

                    documentsResponse.Add(documentResponse);
                }
            }

            transaction.Commit();
            return documentsResponse;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<PatientDocumentResponse>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();

        try
        {
            var sb = new StringBuilder();
            sb.Append("SELECT * FROM Documents ");
            var query = sb.ToString();

            return (await connection.QueryAsync<PatientDocument>(query)).ToList().Select(d =>
                d.ToDocumentResponse(_dbConfigs.GetFullSaveFolderPathForDocuments(d.Name)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<PatientDocumentResponse?> GetByIdAsync(int id) =>
        (await GetAllAsync()).SingleOrDefault(d => d.Id == id);

    public async Task<PatientDocumentResponse> DeleteByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();

        var documentToDelete = await GetByIdAsync(id);
        if (documentToDelete == null)
        {
            return null!;
        }

        var sb = new StringBuilder();
        sb.Append("DELETE FROM Documents ");
        sb.Append("WHERE id= @id");
        var query = sb.ToString();

        await _fileRepository.DeleteFilesAsync(documentToDelete.Name); 

        await connection.QueryFirstOrDefaultAsync<PatientDocument>(query, new { id = id });

        return documentToDelete;
    }

    public async Task<IEnumerable<PatientDocumentResponse>> DeleteByVisitIdAsync(int visitId)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        
      
        var sb = new StringBuilder();
        sb.Append("SELECT * FROM Documents ");
        sb.Append("WHERE Visitid= @id");
        var query = sb.ToString();

        var documentsToDelete = await connection.QueryAsync<PatientDocument>(query, new { VisitId = visitId });

        if (documentsToDelete.Count() == 0)
        {
            return null;
        }
        
        var deleteQuery = new StringBuilder();
        deleteQuery.Append("DELETE FROM Documents ");
        deleteQuery.Append("WHERE VisitId= @id");
       
        var deteledDocuments = await connection.QueryAsync<PatientDocument>(deleteQuery.ToString(), new { VisitId = visitId });

        return documentsToDelete.ToList()
            .Select(d => d.ToDocumentResponse(_dbConfigs.GetFullSaveFolderPathForDocuments(d.Name)));
    }

    private string GenerateDocumentPath(int visitId, string originalFileExtension)
    {
        var currentTimeMs = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString();
        var timeStamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{visitId}_{timeStamp}_{currentTimeMs}";

        return Path.Combine(fileName + originalFileExtension);
    }
}
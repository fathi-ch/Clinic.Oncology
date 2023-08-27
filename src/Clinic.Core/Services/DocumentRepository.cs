﻿using System.Reflection.Metadata;
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
            int newId = 0;
            var documentsResponse = new List<PatientDocumentResponse>();
            if (patientDocumentDto.Files.Count() > 0)
            {
                foreach (var file in patientDocumentDto.Files)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO ");
                    sb.Append("Documents (VisitId, Name, DocumentType) ");
                    sb.Append("VALUES (@VisitId, @Name, @DocumentType); ");
                    sb.Append("SELECT last_insert_rowid();");
                    
                    var query = sb.ToString();
                    var originalFileExtension = Path.GetExtension(file.FileName);
                    var documentsFullName = GenerateDocumentPath(patientDocumentDto.VisitId, originalFileExtension);
                    newId = await connection.ExecuteScalarAsync<int>(query,
                        new
                        {
                            VisitId = patientDocumentDto.VisitId, Name = documentsFullName,
                            DocumentType = patientDocumentDto.DocumentType
                        });

                    await _fileRepository.SaveFilesAsync(file, documentsFullName);

                    var documentResponse = patientDocumentDto.ToDocumentResponse(newId, documentsFullName,
                        _dbConfigs.GetFullSaveFolderPathForDocuments(documentsFullName));

                    documentsResponse.Add(documentResponse);
                    
                }
                transaction.Commit();
            }


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

    public async Task<IEnumerable<PatientDocumentResponse>> GetByVisitIdAsync(int visitId)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();

        var sb = new StringBuilder();
        sb.Append("SELECT * FROM Documents ");
        sb.Append("WHERE VisitId = @VisitId");
        var query = sb.ToString();

        return (await connection.QueryAsync<PatientDocument>(query, new { VisitId = visitId })).ToList().Select(d =>
            d.ToDocumentResponse(_dbConfigs.GetFullSaveFolderPathForDocuments(d.Name)));
    }

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

        try
        {
            var sb1 = new StringBuilder();
            sb1.Append("DELETE FROM Documents ");
            sb1.Append("WHERE VisitId = @VisitId");
            var deleteFromDoucments = sb1.ToString();

            var result = (await GetAllAsync()).Where(d => d.VisitId == visitId);
            
            var documentsToDeleteClone = result.Select(d => new PatientDocumentResponse 
            {
                Id = d.Id,
                Name = d.Name,
                VisitId = d.VisitId,
                DocumentType = d.DocumentType,
                PatientDocumentsbase64 = d.PatientDocumentsbase64
            }).ToList();

            var documents = await connection.QueryAsync<PatientDocument>(deleteFromDoucments, new { VisitId = visitId });

            List<Task> tasks = new List<Task>();
            foreach (var doc in result)
            {
                tasks.Add(_fileRepository.DeleteFilesAsync(doc.Name));
            }


            var sb = new StringBuilder();
            sb.Append("DELETE ");
            sb.Append("FROM Visits ");
            sb.Append("WHERE Id = @VisitId;");
            var query = sb.ToString();


            var deletedVisits = await connection.QueryAsync<PatientDocument>(query, new { VisitId = visitId });
            await Task.WhenAll(tasks);
            return documentsToDeleteClone;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private string GenerateDocumentPath(int visitId, string originalFileExtension)
    {
        var currentTimeMs = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString();
        var timeStamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{visitId}_{timeStamp}_{currentTimeMs}";

        return Path.Combine(fileName + originalFileExtension);
    }
}
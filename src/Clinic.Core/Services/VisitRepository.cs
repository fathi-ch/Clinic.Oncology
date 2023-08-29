using System.Text;
using Clinic.Core.Contracts;
using Clinic.Core.Data;
using Clinic.Core.Dto;
using Clinic.Core.Mappers;
using Clinic.Core.Models;
using Dapper;

namespace Clinic.Core.Services;

public class VisitRepository : IVisitRepository
{
    private readonly ISqliteDbConnectionFactory _connectionFactory;
    private readonly IDocumentRepository _documentRepository;

    public VisitRepository(ISqliteDbConnectionFactory connectionFactory, IDocumentRepository documentRepository)
    {
        _connectionFactory = connectionFactory;
        _documentRepository = documentRepository;
    }

    public async Task<VisitResponse> CreateAsync(VisitDto visitDto)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO Visits (PatientId, StartTime, EndTime, Price, Description, VisitType, Status) ");
            sb.Append("VALUES (@PatientId, @StartTime, @EndTime, @Price, @Description, @VisitType, @Status);");
            sb.Append("SELECT last_insert_rowid();");
            var query = sb.ToString();

            var newId = await connection.ExecuteScalarAsync<int>(query,
                new
                {
                    visitDto.PatientId, visitDto.StartTime, visitDto.EndTime, visitDto.Price, visitDto.Description,
                    visitDto.VisitType, visitDto.Status
                });

            transaction.Commit();

            return visitDto.ToVisitResponse(newId);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<VisitResponse>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();

        var sb = new StringBuilder();
        sb.Append("SELECT * ");
        sb.Append("FROM Visits ");

        var query = sb.ToString();
        var result = await connection.QueryAsync<Visit>(query);

        var visitTasks = result.Select(async x =>
        {
            var documents = await _documentRepository.GetByVisitIdAsync(x.Id);
            return x.ToVisitResponse(documents);
        }).ToList();

        var visitResponses = await Task.WhenAll(visitTasks);

        return visitResponses;
    }

    public async Task<VisitResponse> GetByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        var sb = new StringBuilder();
        sb.Append("SELECT * ");
        sb.Append("FROM Visits v ");
        sb.Append("WHERE v.Id = @VisitId;");

        var query = sb.ToString();
        var result = await connection.QueryFirstOrDefaultAsync<Visit>(
            query,
            new { VisitId = id });

        return result.ToVisitResponse(await _documentRepository.GetByVisitIdAsync(id));
    }

    public async Task<VisitResponse> DeleteByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();

        var visitToDelete = await GetByIdAsync(id);
        if (visitToDelete == null)
        {
            return null;
        }


        var listOfDocs = await _documentRepository.DeleteByVisitIdAsync(id);

        var deleteQuery = new StringBuilder();
        deleteQuery.Append("DELETE FROM Visits ");
        deleteQuery.Append("WHERE Id= @Id");
        var query = deleteQuery.ToString();

        try
        {
            await connection.ExecuteAsync(query, new { Id = id });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        visitToDelete.Documents = listOfDocs;
        return visitToDelete;
    }

    public async Task<VisitResponse> UpdateByIdAsync(int id, VisitDto visitDto)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var sb = new StringBuilder();
            sb.Append("UPDATE Visits SET ");
            sb.Append("PatientId = @PatientId, ");
            sb.Append("StartTime = @StartTime, ");
            sb.Append("EndTime = @EndTime, ");
            sb.Append("Price = @Price, ");
            sb.Append("Description = @Description, ");
            sb.Append("VisitType = @VisitType, ");
            sb.Append("Status = @Status ");
            sb.Append("WHERE Id = @id;");

            var query = sb.ToString();

            await connection.ExecuteAsync(query,
                new
                {
                    id = id, PatientId = visitDto.PatientId, StartTime = visitDto.StartTime, EndTime = visitDto.EndTime,
                    Price = visitDto.Price, Description = visitDto.Description,
                    VisitType = visitDto.VisitType, Status = visitDto.Status
                });

            transaction.Commit();

            return visitDto.ToVisitResponse(id);
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}
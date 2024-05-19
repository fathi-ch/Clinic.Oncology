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
    private readonly IPatientRepository _patientRepository;

    public VisitRepository(ISqliteDbConnectionFactory connectionFactory, IDocumentRepository documentRepository, IPatientRepository patientRepository)
    {
        _connectionFactory = connectionFactory;
        _documentRepository = documentRepository;
        _patientRepository = patientRepository;
    }

    public async Task<VisitResponse> CreateAsync(VisitDto visitDto)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO Visits (PatientId, StartTime, EndTime, Price, Description, VisitType, Status,Weight,Height) ");
            sb.Append("VALUES (@PatientId, @StartTime, @EndTime, @Price, @Description, @VisitType, @Status,@Weight,@Height);");
            sb.Append("SELECT last_insert_rowid();");
            var query = sb.ToString();

            var newId = await connection.ExecuteScalarAsync<int>(query,
                new
                {
                    visitDto.Id,
                    visitDto.PatientId,
                    visitDto.StartTime,
                    visitDto.EndTime,
                    visitDto.Price,
                    visitDto.Description,
                    visitDto.VisitType,
                    visitDto.Status,
                    visitDto.Weight,
                    visitDto.Height
                });

            var all = await this.GetAllAsync();
            all = all.Where(visit => visit.PatientId == visitDto.PatientId);
            if (all.Any())
            {
                var nextDate = all.Max(visit => visit.StartTime);

                sb = new StringBuilder();
                sb.Append("UPDATE Patients set ");
                sb.Append("NextAppointment = @NextAppointment ");
                sb.Append("WHERE Id = @id;");

                query = sb.ToString();

                await connection.ExecuteAsync(query,
                    new
                    {
                        NextAppointment = nextDate,
                        id = visitDto.PatientId

                    });
            }

            transaction.Commit();

            return visitDto.ToVisitResponse();
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

        return result.ToVisitResponse();
    }

    public async Task<IEnumerable<VisitResponse>> GetBydDateAsync(DateTime fromDate, DateTime toDate)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();

        var sb = new StringBuilder();
        sb.Append("SELECT * ");
        sb.Append("FROM Visits WHERE StartTime between @StartTime and @EndTime");

        var query = sb.ToString();
        var result = await connection.QueryAsync<Visit>(query, new {StartTime= fromDate , EndTime= toDate });

        return result.ToVisitResponse();
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

        return result.ToVisitResponse();
    }

    public async Task<VisitResponse> DeleteByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
      

        var visitToDelete = await GetByIdAsync(id);
        if (visitToDelete == null)
        {
            return null;
        }

        var visit = await GetByIdAsync(id);

        var listOfDocs = await _documentRepository.DeleteByVisitIdAsync(id);

        var deleteQuery = new StringBuilder();
        deleteQuery.Append("DELETE FROM Visits ");
        deleteQuery.Append("WHERE Id= @Id");
        var query = deleteQuery.ToString();

        try
        {
            await connection.ExecuteAsync(query, new { Id = id });


            var all = await this.GetAllAsync();
            all = all.Where(visit => visit.PatientId == visitToDelete.PatientId);
            if (all.Any())
            {
                var nextDate = all.Max(visit => visit.StartTime);

                var sb = new StringBuilder();
                sb.Append("UPDATE Patients set ");
                sb.Append("NextAppointment = @NextAppointment ");
                sb.Append("WHERE Id = @id;");

                query = sb.ToString();

                await connection.ExecuteAsync(query,
                    new
                    {
                        NextAppointment = nextDate,
                        id = visitToDelete.PatientId

                    });
            }

            visitToDelete.Documents = listOfDocs;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        
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
            sb.Append("Status = @Status, ");
            sb.Append("Weight = @Weight, ");
            sb.Append("Height = @Height ");
            sb.Append("WHERE Id = @id;");

            var query = sb.ToString();

            await connection.ExecuteAsync(query,
                new
                {
                    id = id,
                    PatientId = visitDto.PatientId,
                    StartTime   = visitDto.StartTime,
                    EndTime     = visitDto.EndTime,
                    Price       = visitDto.Price,
                    Description = visitDto.Description,
                    VisitType   = visitDto.VisitType,
                    Weight = visitDto.Weight,
                    Height = visitDto.Height,
                    Status = visitDto.Status,
                });
            transaction.Commit();
           

            return visitDto.ToVisitResponse(id);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw;
        }
    }
}
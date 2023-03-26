using Microsoft.AspNetCore.Mvc.RazorPages;
using Clinic.Core.Contracts;
using Clinic.Core.Data;
using Clinic.Core.Mappers;
using Clinic.Core.Models;
using Clinic.Core.Repositories;
using Dapper;


namespace Presentation.Pages.PatientList
{
    public class IndexModel : PageModel
    {
        private readonly ISqliteDbConnectionFactory _connectionFactory;
        private readonly IPatientRepository _patient;

        public IndexModel(ISqliteDbConnectionFactory connectionFactory, IPatientRepository patient)
        {
            _connectionFactory = connectionFactory;
            _patient = patient;
        }

        public IEnumerable<PatientResponse> Patientlist { get; set; }

        public async Task OnGet()
        {
            var patients = await _patient.GetAllAsync();
            Patientlist = patients.Select(x => x.ToPatientResponse());
        }

        // private async Task<IQueryable<PatientResponse>> GetAllPatients()
        // {
        //     using var connection = await _connectionFactory.CreateDbConnectionAsync();
        //     var patients = await connection.QueryAsync<Patient>("select * from Patients");
        //
        //     if (!patients.Any())
        //     {
        //         return Enumerable.Empty<PatientResponse>().AsQueryable();
        //     }
        //
        //     var tasks = patients.Select(async x => new PatientResponse
        //     {
        //         Id = x.Id,
        //         FirstName = x.FirstName,
        //         LastName = x.LastName,
        //         BirthDate = x.BirthDate,
        //         TotalDocuments = await GetTotalDocumentAsync(x),
        //         NextAppointment = x.NextAppointment
        //     });
        //     var patientResponses = await Task.WhenAll(tasks);
        //
        //     return await Task.FromResult(patientResponses.AsQueryable());
        // }
        //
        // private async Task<int> GetTotalDocumentAsync(Patient patient)
        // {
        //     using var connection = await _connectionFactory.CreateDbConnectionAsync();
        //
        //     var totalDocument = await connection.QueryFirstOrDefaultAsync<int>(
        //         "SELECT COUNT(d.Id) AS NumDocuments " +
        //         "FROM Patients p " +
        //         "LEFT JOIN Documents d ON p.Id = d.PatientId " +
        //         "WHERE d.PatientId = @PatientId;",
        //         new { PatientId = patient.Id });
        //
        //     return await Task.FromResult(totalDocument);
        // }
    }
}
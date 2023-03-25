using Clinic.Core.Contracts;
using Clinic.Core.Data;
using Clinic.Core.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Pages.PatientList
{
    public class IndexModel : PageModel
    {
        private readonly ISqliteDbConnectionFactory _connectionFactory;

        public IndexModel(ISqliteDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<PatientResponse> Patients { get; set; }
        public async Task OnGet()
        {
            Patients = await GetAllPatients();
        }

        private async Task<IQueryable<PatientResponse>> GetAllPatients()
        {
            using var connection = await _connectionFactory.CreateDbConnectionAsync();
            var patients = await connection.QueryAsync<Patient>("select * from Patients");

            if (!patients.Any())
            {
                return Enumerable.Empty<PatientResponse>().AsQueryable();
            }


            var tasks = patients.Select(async x => new PatientResponse
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Age = (int)(DateTime.UtcNow.Subtract(x.BirthDate).TotalDays / 365.25),
                TotalDocuments = await GetTotalDocumentAsync(x)

            });

            var patientResponses = await Task.WhenAll(tasks);

            return await Task.FromResult(patientResponses.AsQueryable());



        }

        private async Task<int> GetTotalDocumentAsync(Patient patient)
        {
            using var connection = await _connectionFactory.CreateDbConnectionAsync();

            var totalDocument = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(d.Id) AS NumDocuments " +
                "FROM Patients p " +
                "LEFT JOIN Documents d ON p.Id = d.PatientId " +
                "WHERE d.PatientId = @PatientId;",
                new { PatientId = patient.Id });


             return await Task.FromResult(totalDocument);
            //return totalDocument;

        }
    }
}


using Clinic.Core.Data;
using Clinic.Core.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;

namespace Presentation.Pages.PatientList
{
    public class EditModel : PageModel
    {
        private readonly ISqliteDbConnectionFactory _connectionFactory;

        [BindProperty]
        public string base64Image { get; set; }
        [BindProperty]
        public List<PatientDocument> Documents { get; set; }

        public EditModel(ISqliteDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task OnGet(string id)
        {
            var result = await GetPatientByIdAsync(id);
            Documents = result.Documents;
            var image = Documents.FirstOrDefault();
            byte[] imageByte = System.IO.File.ReadAllBytes(image.Path);
            base64Image = Convert.ToBase64String(imageByte);    



            
            //base64Image = result.Patient.FirstName;
        }

        private async Task<PatientViewModel> GetPatientByIdAsync(string id)
        {
            using var connection = await _connectionFactory.CreateDbConnectionAsync();

            var patient = await connection.QueryFirstOrDefaultAsync<Patient>(
                "SELECT * " +
                "FROM Patients p " +
                "WHERE p.Id = @PatientId;",
                new { PatientId = Guid.Parse(id) });

            var documents = await connection.QueryAsync<PatientDocument>(
                "SELECT d.Id, d.PatientId, d.Path " +
                "FROM Patients p " +
                "LEFT JOIN Documents d ON p.Id = d.PatientId " +
                "WHERE d.PatientId = @PatientId;",
                 new { PatientId = Guid.Parse(id) });

            var patientViewModel = new PatientViewModel
            {
                Patient = patient,
                Documents = documents.ToList()
            };


            return await Task.FromResult(patientViewModel);
        }
    }
}

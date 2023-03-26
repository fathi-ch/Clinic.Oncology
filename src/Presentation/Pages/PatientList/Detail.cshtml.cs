using Clinic.Core.Data;
using Clinic.Core.Models;
using Clinic.Core.ViewModels;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Pages.PatientList
{
    public class DetailModel : PageModel
    {
        private readonly ISqliteDbConnectionFactory _connectionFactory;

        [BindProperty]
        public List<string> base64Images { get; set; }

        [BindProperty]
        public PatientViewModel PatientDetails { get; set; }

        public DetailModel(ISqliteDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            base64Images = new List<string>();          
        }
        public async Task OnGet(string id)
        {
            PatientDetails = await GetPatientByIdAsync(id);

           
            foreach (var image in PatientDetails.Documents)
            {
                byte[] imageByte = System.IO.File.ReadAllBytes(image.Path);
                base64Images.Add(Convert.ToBase64String(imageByte));

            }

        }

        private async Task<PatientViewModel> GetPatientByIdAsync(string id)
        {
            using var connection = await _connectionFactory.CreateDbConnectionAsync();

            //Maybe join both table to One query in the future
            var patient = await connection.QueryFirstOrDefaultAsync<Patient>(
               "SELECT * " +
               "FROM Patients p " +
               "WHERE p.Id = @PatientId;",
               new { PatientId = Guid.Parse(id) });

            var documents = await connection.QueryAsync<PatientDocument>(
                "SELECT * " +
                "FROM Documents " +
                "WHERE PatientId = @PatientId;",
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
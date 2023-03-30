using Clinic.Core.Data;
using Clinic.Core.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using Clinic.Core.Repositories;

namespace Presentation.Pages.PatientList
{
    public class CreateModel : PageModel
    {
        private readonly ISqliteDbConnectionFactory _connectionFactory;
        private readonly IPatientRepository _patient;

        [BindProperty]
        public Patient Patient { get; set; }

        public CreateModel(ISqliteDbConnectionFactory connectionFactory, IPatientRepository patient)
        {
            _connectionFactory = connectionFactory;
            _patient = patient;
        }
      
        public async Task<IActionResult> OnPost(IEnumerable<IFormFile> files)
        {
            await _patient.CreateWithDocumentsAsync(Patient, files);
            return RedirectToPage("Index");
        }
    }
}

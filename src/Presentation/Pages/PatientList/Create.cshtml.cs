using Clinic.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Clinic.Core.Repositories;

namespace Presentation.Pages.PatientList
{
    public class CreateModel : PageModel
    {
        private readonly IPatientRepository _patient;
        [BindProperty] public Patient Patient { get; set; }
        public CreateModel(IPatientRepository patient)
        {
            _patient = patient;
        }
        public async Task<IActionResult> OnPost(IEnumerable<IFormFile> files)
        {
            //No validation so far till now it may take part during the frontEnd development
            await _patient.CreateWithDocumentsAsync(Patient, files);
            return RedirectToPage("Index");
        }
    }
}

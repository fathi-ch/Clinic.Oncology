using Clinic.Core.Models;
using Clinic.Core.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Pages.PatientList;

public class CreateModel : PageModel
{
    private readonly IPatientRepository _patient;

    public CreateModel(IPatientRepository patient)
    {
        _patient = patient;
    }

    [BindProperty] public Patient Patient { get; set; }

    public async Task<IActionResult> OnPost(IEnumerable<IFormFile> files)
    {
        //No validation so far till now it may take part during the frontEnd development
        await _patient.CreateWithDocumentsAsync(Patient, files);
        return RedirectToPage("Index");
    }
}
using Microsoft.AspNetCore.Mvc.RazorPages;
using Clinic.Core.Contracts;
using Clinic.Core.Mappers;
using Clinic.Core.Repositories;


namespace Presentation.Pages.PatientList
{
    public class IndexModel : PageModel
    {
        private readonly IPatientRepository _patient;

        public IndexModel(IPatientRepository patient)
        {
            _patient = patient;
        }
        public IEnumerable<PatientResponse> Patientlist { get; set; }
        public async Task OnGet()
        {
            var patients = await _patient.GetAllAsync();
            Patientlist = patients.Select(x => x.ToPatientResponse());
        }
    }
}
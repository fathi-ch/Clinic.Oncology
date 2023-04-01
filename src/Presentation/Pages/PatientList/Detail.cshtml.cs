using Clinic.Core.Contracts;
using Clinic.Core.Data;
using Clinic.Core.Mappers;
using Clinic.Core.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Pages.PatientList
{
    public class DetailModel : PageModel
    {
        private readonly IPatientRepository _patient;
        private readonly IPatientDocumentRepository _patientDocument;
        [BindProperty] public List<PatientDocumentResponse> DocumentResponses { get; set; }
        [BindProperty] public PatientResponse PatientResponseDetails { get; set; }

        public DetailModel(IPatientRepository patient,
            IPatientDocumentRepository patientDocument)
        {
            _patient = patient;
            _patientDocument = patientDocument;
        }

        public async Task OnGet(string id)
        {
            var patient = await _patient.GetByIdAsync(id);
            PatientResponseDetails =  patient.ToPatientResponse();

            var patientDocuments = await _patientDocument.GetPatientDocumentByPatientId(id);
            DocumentResponses = patientDocuments.Select(x => x.ToDocumentResponse()).ToList();
        }
    }
}
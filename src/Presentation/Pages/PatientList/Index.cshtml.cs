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
    }
}
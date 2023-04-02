using Microsoft.AspNetCore.Mvc.RazorPages;
using Clinic.Core.Contracts;

namespace Presentation.Pages.PatientList
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient = new()
        {
            //This will be refactored FrontEnd to not hardcode the api url
            BaseAddress = new Uri("https://localhost:7017")
        };
        public IEnumerable<PatientResponse>? Patients { get; set; }
        public async Task OnGet()
        {
            Patients = await _httpClient.GetFromJsonAsync<List<PatientResponse>>("/v1/api/Patients");
        }
    }
}
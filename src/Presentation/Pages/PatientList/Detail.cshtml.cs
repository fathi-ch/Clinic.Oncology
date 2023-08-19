using Clinic.Core.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Pages.PatientList;

public class DetailModel : PageModel
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:7017")
    };

    public IEnumerable<PatientDocumentResponse>? PatientDocsResponse { get; set; }
    public PatientResponse? PatientResponse { get; set; }

    public async Task OnGet(string id)
    {
        try
        {
            PatientResponse = await _httpClient.GetFromJsonAsync<PatientResponse>($"/v1/api/Patients/{id}");
            PatientDocsResponse =
                await _httpClient.GetFromJsonAsync<IEnumerable<PatientDocumentResponse>>(
                    $"/v1/api/patient/{id}/documents");
        }
        catch (HttpRequestException ex)
        {
            PatientDocsResponse = null;
        }
    }
}
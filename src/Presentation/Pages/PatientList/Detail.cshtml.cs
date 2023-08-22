using Clinic.Core.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Pages.PatientList;

public class DetailModel : PageModel
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:7017")
    };

    public PatientWithDocumentsResponse PatientDocsResponse { get; set; }

    public async Task OnGet(string id)
    {
        try
        {
            PatientDocsResponse =
                await _httpClient.GetFromJsonAsync<PatientWithDocumentsResponse>($"/v1/api/patient/{id}/documents");
        }
        catch (HttpRequestException ex)
        {
            PatientDocsResponse = null;
        }
    }
}
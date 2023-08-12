using System.Text.Json;
using Clinic.Core.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Pages.PatientList;

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
        try
        {
            Patients = await _httpClient.GetFromJsonAsync<List<PatientResponse>>("/v1/api/Patients");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e.Message);
            Patients = Enumerable.Empty<PatientResponse>();
        }
        catch (JsonException e)
        {
            Console.WriteLine(e.Message);
            Patients = Enumerable.Empty<PatientResponse>();
        }
        catch (NullReferenceException e)
        {
            Console.WriteLine(e.Message); //Maybe DataBase is Empty or Unavailable 
            Patients = Enumerable.Empty<PatientResponse>();
        }
    }
}
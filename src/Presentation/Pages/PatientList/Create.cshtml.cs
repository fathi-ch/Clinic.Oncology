using System.Text;
using Clinic.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Presentation.Pages.PatientList;

public class CreateModel : PageModel
{
    [BindProperty] public Patient Patient { get; set; }

    private readonly HttpClient _httpClient = new()
    {
        //This will be refactored FrontEnd to not hardcode the api url
        BaseAddress = new Uri("http://localhost:7017")
    };

    public async Task<IActionResult> OnPost(IEnumerable<IFormFile> files)
    {
        var multipartContent = new MultipartFormDataContent();
        var patientJson = JsonConvert.SerializeObject(Patient);
        multipartContent.Add(new StringContent(patientJson, Encoding.UTF8, "application/json"), "Patient");

        // Add the files
        foreach (var file in files)
        {
            var streamContent = new StreamContent(file.OpenReadStream())
            {
                Headers =
                {
                    ContentLength = file.Length,
                    ContentType = new MediaTypeHeaderValue(file.ContentType)
                }
            };

            multipartContent.Add(streamContent, "files", file.FileName);
        }

        var response = await _httpClient.PostAsync("/v1/api/Patients", multipartContent);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("Index");
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}). Content: {errorContent}");
        }
    }
}
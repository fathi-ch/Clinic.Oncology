using Microsoft.AspNetCore.Http;

namespace Clinic.Core.Models;

public class PatientDocumentsUploadModel
{
    public string Patient { get; set; }

    public IFormFileCollection? Files { get; set; }
}
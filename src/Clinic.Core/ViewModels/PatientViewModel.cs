using Clinic.Core.Models;


namespace Clinic.Core.ViewModels
{
    public class PatientViewModel
    {
        public Patient Patient { get; set; }
        public List<PatientDocument> Documents { get; set; }
    }
}

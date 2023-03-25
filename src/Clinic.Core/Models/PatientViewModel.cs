namespace Clinic.Core.Models
{
    public class PatientViewModel
    {
        public Patient Patient { get; set; }
        public List<PatientDocument> Documents { get; set; }
    }

    public class PatientDocument
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Path { get; set; }
    }
}

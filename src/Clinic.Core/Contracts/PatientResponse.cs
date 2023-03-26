using System.ComponentModel;
using Clinic.Core.Models;

namespace Clinic.Core.Contracts
{
    public class PatientResponse
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Age  => (int)(DateTime.UtcNow.Subtract(BirthDate).TotalDays / 365.25);
        public  DateTime BirthDate { get; set; }
        public  DateTime NextAppointment { get; set; }
        public int TotalDocuments { get; set; }
        public List<PatientDocument> Documents { get; set; }
        public string GetPatientFullName() => string.Concat(FirstName," " , LastName);

    }
}

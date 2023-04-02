using System.ComponentModel;
using System.Text.Json.Serialization;
using Clinic.Core.Models;

namespace Clinic.Core.Contracts
{
    public class PatientResponse
    {
        public Guid Id { get; set; }   
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Age { get; set; }
        public  DateTime BirthDate { get; set; }
        public  DateTime NextAppointment { get; set; }
        public string GetPatientFullName() => string.Concat(FirstName," " , LastName);

    }
}

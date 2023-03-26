#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Clinic.Core.Models
{
    public class Patient
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime BirthDate { get; set; }
        
        public string GetPatientFullName() => string.Concat(FirstName," " , LastName);
    }
}

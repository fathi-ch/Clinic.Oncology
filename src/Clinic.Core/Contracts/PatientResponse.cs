using System.ComponentModel.DataAnnotations;

namespace Clinic.Core.Contracts;

public class PatientResponse
{
    public Guid Id { get; set; }
    
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
    [Display(Name = "Next Appointment")]
    public DateTime NextAppointment { get; set; }

    public string GetPatientFullName()
    {
        return string.Concat(FirstName, " ", LastName);
    }
}
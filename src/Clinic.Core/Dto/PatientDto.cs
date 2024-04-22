namespace Clinic.Core.Dto;

public class PatientDto
{
    public int id { get; set; }
    public int Age { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime? NextAppointment { get; set; }
    public string? Gender { get; set; }
    public float Weight{ get; set; }
    public float Height { get; set; }
    public string? Mobile { get; set; }
    public string? SocialSecurityNumber { get; set; }
    public string? Referral { get; set; }
}
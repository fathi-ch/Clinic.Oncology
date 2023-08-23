#nullable enable
namespace Clinic.Core.Models;

public class Patient
{
    public int Id { get; set; }
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
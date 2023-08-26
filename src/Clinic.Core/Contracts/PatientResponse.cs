using System.Runtime.CompilerServices;

namespace Clinic.Core.Contracts;

public class PatientResponse
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime? NextAppointment { get; set; }
    public string? Gender { get; set; }
    public float Weight{ get; set; }
    public float Height { get; set; }
    public string? Mobile { get; set; }
    public string? SocialSecurityNumber { get; set; }
    public string? Referral { get; set; }
    public string AutocpmliteValue { get { return  string.Format("{0} ,{1}    {2}", this.FirstName,this.LastName,this.BirthDate.ToString("dd/MM/yyyy")); } }
    
}
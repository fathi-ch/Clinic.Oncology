namespace Clinic.Core.Models;

public class Visit
{
    public int Id { get; set; }
    public int PatientId { get; set; } 
    public Patient Patient { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public float Price { get; set; }
    public string Description { get; set; }
    public string VisitType { get; set; }
    public string Status { get; set; }
    public float Weight { get; set; }
    public float Height { get; set; }
    public string visitDateTitle { get; set; }
    //public List<PatientDocument> Documents { get; set; } 
}
﻿using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic.Core.Dto;

public class VisitDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public float Price { get; set; }
    public string Description { get; set; }
    public string VisitType { get; set; }
    public string Status { get; set; }
    public float Weight { get; set; }
    public float Height { get; set; }
    public string visitDateTitel { get; set; }
    //public IEnumerable<IFormFile?> Documents { get; set; }
}
﻿using Clinic.Core.Contracts;
using Clinic.Core.Mappers;
using Clinic.Core.Models;
using Clinic.Core.Repositories;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("/v1/api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientRepository _patientRepository;

    public PatientsController(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository ??
                             throw new ArgumentException(null, nameof(patientRepository));
    }

    [HttpGet(Name = "GetAllPatients")]
    public async Task<ActionResult<IEnumerable<PatientResponse>>> GetAll()
    {
        var result = await _patientRepository.GetAllAsync();
        var patients = result.ToList();
        if (!patients.Any()) return NotFound();
        return Ok(patients.Select(p => p.ToPatientResponse()));
    }

    [HttpGet("{id}", Name = "GetPatient")]
    public async Task<ActionResult<PatientResponse>> Get(string id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null) return NotFound();
        return Ok(patient.ToPatientResponse());
    }

    [HttpPost(Name = "CreateWithDocuments")]
    public async Task<IActionResult> CreateWithDocumentsAsync([FromForm] PatientDocumentsUploadModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var patient = JsonConvert.DeserializeObject<Patient>(model.Patient);
        var result = await _patientRepository.CreateWithDocumentsAsync(patient, model.Files);

        if (result) return Created("Created", result);

        return StatusCode(StatusCodes.Status500InternalServerError);
    }
}
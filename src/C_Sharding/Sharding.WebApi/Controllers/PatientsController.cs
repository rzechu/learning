using Microsoft.AspNetCore.Mvc;
using Sharding.WebApi.Models;
using Sharding.WebApi.Services;

namespace Sharding.WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public PatientsController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<Patient>> Register(Patient newPatient)
    {
        try
        {
            var registeredPatient = await _mongoDBService.RegisterPatientAsync(newPatient);
            return CreatedAtAction(nameof(GetPatientInfo),
                new { clinicId = registeredPatient.ClinicId, patientId = registeredPatient.PatientId },
                registeredPatient);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message); // 409 Conflict for duplicate patient ID
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("{clinicId}/{patientId}")]
    public async Task<ActionResult<Patient>> GetPatientInfo(string clinicId, string patientId)
    {
        var patient = await _mongoDBService.GetPatientInfoAsync(clinicId, patientId);
        if (patient == null)
        {
            return NotFound($"Patient with ID '{patientId}' not found in clinic '{clinicId}'.");
        }
        return patient;
    }
}
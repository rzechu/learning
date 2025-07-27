using Microsoft.AspNetCore.Mvc;
using Sharding.WebApi.Models;
using Sharding.WebApi.Services;

namespace Sharding.WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class VisitsController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public VisitsController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    [HttpPost("book")]
    public async Task<ActionResult<Visit>> Book(Visit newVisit)
    {
        try
        {
            var bookedVisit = await _mongoDBService.BookVisitAsync(newVisit);
            return CreatedAtAction(nameof(GetPatientVisits),
                new { clinicId = bookedVisit.ClinicId, patientId = bookedVisit.PatientId },
                bookedVisit);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("{clinicId}/{patientId}")]
    public async Task<ActionResult<List<Visit>>> GetPatientVisits(string clinicId, string patientId)
    {
        var visits = await _mongoDBService.GetPatientVisitsAsync(clinicId, patientId);
        if (visits == null || visits.Count == 0)
        {
            // It's possible for a patient to exist but have no visits yet.
            // You might return an empty list or a 404 if the patient itself isn't found
            // (though GetPatientInfo already handles patient not found).
            return Ok(new List<Visit>()); // Return empty list if no visits found
        }
        return visits;
    }
}
using Microsoft.AspNetCore.Mvc;
using Sharding.WebApi.Models;
using Sharding.WebApi.Services;

namespace Sharding.WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ClinicsController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public ClinicsController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Clinic>>> Get() =>
        await _mongoDBService.GetClinicsAsync();

    [HttpGet("{clinicId}")]
    public async Task<ActionResult<Clinic>> GetClinicById(string clinicId)
    {
        var clinic = await _mongoDBService.GetClinicByIdAsync(clinicId);
        if (clinic == null)
        {
            return NotFound($"Clinic with ID '{clinicId}' not found.");
        }
        return clinic;
    }
}
using Sharding.WebApi.Models;
using MongoDB.Driver;

namespace Sharding.WebApi.Services;

public class MongoDBService
{
    private readonly IMongoCollection<Clinic> _clinicsCollection;
    private readonly IMongoCollection<Patient> _patientsCollection;
    private readonly IMongoCollection<Visit> _visitsCollection;

    public MongoDBService(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);

        _clinicsCollection = database.GetCollection<Clinic>("Clinics");
        _patientsCollection = database.GetCollection<Patient>("Patients");
        _visitsCollection = database.GetCollection<Visit>("Visits");

        // Seed initial data (Clinics) if they don't exist
        SeedClinicsAsync().Wait(); // Using .Wait() for simplicity in POC,
                                   // in real app, handle async properly during startup
    }

    private async Task SeedClinicsAsync()
    {
        if (await _clinicsCollection.EstimatedDocumentCountAsync() == 0)
        {
            var clinics = new List<Clinic>
                {
                    new Clinic { ClinicId = "clinic-001", Name = "Sunrise Health Center", Location = "Kyiv", LicenseNumber = "UA-100012" },
                    new Clinic { ClinicId = "clinic-002", Name = "City Medical Clinic", Location = "Warsaw", LicenseNumber = "PL-200023" },
                    new Clinic { ClinicId = "clinic-003", Name = "Green Valley Hospital", Location = "Berlin", LicenseNumber = "DE-300045" }
                };

            await _clinicsCollection.InsertManyAsync(clinics);
        }
    }

    // --- Clinic Operations ---
    public async Task<List<Clinic>> GetClinicsAsync() =>
        await _clinicsCollection.Find(clinic => true).ToListAsync();

    public async Task<Clinic> GetClinicByIdAsync(string clinicId) =>
        await _clinicsCollection.Find<Clinic>(clinic => clinic.ClinicId == clinicId).FirstOrDefaultAsync();

    // --- Patient Operations ---
    public async Task<Patient> RegisterPatientAsync(Patient newPatient)
    {
        // Ensure ClinicId exists for the new patient
        var clinicExists = await _clinicsCollection.Find(c => c.ClinicId == newPatient.ClinicId).AnyAsync();
        if (!clinicExists)
        {
            throw new KeyNotFoundException($"Clinic with ID '{newPatient.ClinicId}' does not exist.");
        }

        // Optional: Check if PatientId is unique within the clinic
        var patientExistsInClinic = await _patientsCollection.Find(p => p.ClinicId == newPatient.ClinicId && p.PatientId == newPatient.PatientId).AnyAsync();
        if (patientExistsInClinic)
        {
            throw new InvalidOperationException($"Patient with ID '{newPatient.PatientId}' already registered in clinic '{newPatient.ClinicId}'.");
        }

        await _patientsCollection.InsertOneAsync(newPatient);
        return newPatient;
    }

    public async Task<Patient> GetPatientInfoAsync(string clinicId, string patientId) =>
        await _patientsCollection.Find<Patient>(p => p.ClinicId == clinicId && p.PatientId == patientId).FirstOrDefaultAsync();

    // --- Visit Operations ---
    public async Task<Visit> BookVisitAsync(Visit newVisit)
    {
        // Ensure the patient exists within the specified clinic before booking a visit
        var patientExists = await _patientsCollection.Find(p => p.ClinicId == newVisit.ClinicId && p.PatientId == newVisit.PatientId).AnyAsync();
        if (!patientExists)
        {
            throw new KeyNotFoundException($"Patient with ID '{newVisit.PatientId}' not found in clinic '{newVisit.ClinicId}'. Cannot book visit.");
        }

        await _visitsCollection.InsertOneAsync(newVisit);
        return newVisit;
    }

    public async Task<List<Visit>> GetPatientVisitsAsync(string clinicId, string patientId) =>
        await _visitsCollection.Find(v => v.ClinicId == clinicId && v.PatientId == patientId).SortByDescending(v => v.VisitDate).ToListAsync();
}
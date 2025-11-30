using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FMMS.Models;
using FMMS.Helpers;
using SQLite;

namespace FMMS.Services;

/// <summary>
/// SQLite database service for the FMMS app.
/// Handles initialization and CRUD operations for Person, Medication, and other entities.
///
/// Security-related practices:
/// - Soft delete preserves data for audit trails
/// - Validation enforced before all database writes
/// - Uses SQLite-net ORM APIs instead of raw SQL string concatenation
/// - Null checking on all public methods
/// </summary>
public class DatabaseService
{
    private readonly SQLiteAsyncConnection _db;
    private bool _initialized;

    public DatabaseService(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path must not be empty.", nameof(databasePath));
        }

        _db = new SQLiteAsyncConnection(databasePath);
    }

    /// <summary>
    /// Creates tables if they do not exist. Safe to call multiple times.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _db.CreateTableAsync<Person>();
        await _db.CreateTableAsync<Medication>();

        // ScheduleRule tables - SQLite-net doesn't support table inheritance,
        // so FMMS stores each schedule type in its own table and loads them into
        // a single polymorphic list when querying.
        await _db.CreateTableAsync<DailySchedule>();
        await _db.CreateTableAsync<IntervalSchedule>();
        await _db.CreateTableAsync<WeeklySchedule>();
        await _db.CreateTableAsync<AsNeededSchedule>();

        _initialized = true;
    }

    // ==================== PERSON: READ ====================

    /// <summary>
    /// Gets all people from the database, optionally including soft-deleted records.
    /// </summary>
    public async Task<List<Person>> GetPeopleAsync(bool includeDeleted = false)
    {
        await InitializeAsync();

        if (includeDeleted)
        {
            return await _db.Table<Person>()
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();
        }

        return await _db.Table<Person>()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a single person by ID.
    /// </summary>
    public async Task<Person?> GetPersonByIdAsync(int id)
    {
        await InitializeAsync();

        return await _db.Table<Person>()
            .Where(p => p.Id == id && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Searches for people by first name or last name.
    /// Returns multiple rows matching the search criteria.
    /// Satisfies Task 3 search functionality requirement.
    /// </summary>
    public async Task<List<Person>> SearchPeopleAsync(string searchTerm)
    {
        await InitializeAsync();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetPeopleAsync();
        }

        // Sanitize input before using it in search logic
        searchTerm = ValidationHelper.SanitizeInput(searchTerm);
        var term = searchTerm.Trim().ToLowerInvariant();

        var allPeople = await _db.Table<Person>()
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        return allPeople
            .Where(p =>
                (p.FirstName ?? string.Empty).ToLowerInvariant().Contains(term) ||
                (p.LastName ?? string.Empty).ToLowerInvariant().Contains(term) ||
                ($"{p.FirstName} {p.LastName}".Trim()).ToLowerInvariant().Contains(term))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToList();
    }

    // ==================== PERSON: CREATE / UPDATE ====================

    /// <summary>
    /// Saves (inserts or updates) a person.
    /// Validates the entity before saving.
    /// </summary>
    public async Task<int> SavePersonAsync(Person person)
    {
        if (person is null)
        {
            throw new ArgumentNullException(nameof(person));
        }

        await InitializeAsync();

        if (!person.Validate())
        {
            throw new InvalidOperationException("Person entity is not valid.");
        }

        person.MarkAsUpdated();

        if (person.Id != 0)
        {
            return await _db.UpdateAsync(person);
        }

        return await _db.InsertAsync(person);
    }

    // ==================== PERSON: DELETE ====================

    /// <summary>
    /// Soft-deletes the person (marks IsDeleted) and updates the record.
    /// Preserves data for audit trail.
    /// </summary>
    public async Task<int> SoftDeletePersonAsync(Person person)
    {
        if (person is null)
        {
            throw new ArgumentNullException(nameof(person));
        }

        await InitializeAsync();

        person.SoftDelete();
        return await _db.UpdateAsync(person);
    }

    /// <summary>
    /// Hard delete from database (permanently removes the record).
    /// </summary>
    public async Task<int> HardDeletePersonAsync(Person person)
    {
        if (person is null)
        {
            throw new ArgumentNullException(nameof(person));
        }

        await InitializeAsync();

        return await _db.DeleteAsync(person);
    }

    // ==================== MEDICATION: READ ====================

    /// <summary>
    /// Gets all medications from the database, optionally filtered by person.
    /// Can include soft-deleted records.
    /// </summary>
    /// <param name="personId">If provided, only returns medications for this person</param>
    /// <param name="includeDeleted">Whether to include soft-deleted medications</param>
    public async Task<List<Medication>> GetMedicationsAsync(int? personId = null, bool includeDeleted = false)
    {
        await InitializeAsync();

        var query = _db.Table<Medication>();

        // Filter by person if specified
        if (personId.HasValue)
        {
            if (includeDeleted)
            {
                return await query
                    .Where(m => m.PersonId == personId.Value)
                    .OrderBy(m => m.Name)
                    .ToListAsync();
            }
            else
            {
                return await query
                    .Where(m => m.PersonId == personId.Value && !m.IsDeleted)
                    .OrderBy(m => m.Name)
                    .ToListAsync();
            }
        }

        // Get all medications
        if (includeDeleted)
        {
            return await query
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        return await query
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a single medication by ID.
    /// </summary>
    public async Task<Medication?> GetMedicationByIdAsync(int id)
    {
        await InitializeAsync();

        return await _db.Table<Medication>()
            .Where(m => m.Id == id && !m.IsDeleted)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all active medications for a specific person.
    /// Only returns medications where IsActive = true and IsDeleted = false.
    /// </summary>
    public async Task<List<Medication>> GetActiveMedicationsForPersonAsync(int personId)
    {
        await InitializeAsync();

        return await _db.Table<Medication>()
            .Where(m => m.PersonId == personId && m.IsActive && !m.IsDeleted)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets medications that are running low on supply.
    /// Loads all medications and filters using the business logic method.
    /// </summary>
    public async Task<List<Medication>> GetLowSupplyMedicationsAsync(int? personId = null)
    {
        await InitializeAsync();

        var medications = await GetMedicationsAsync(personId, includeDeleted: false);

        return medications
            .Where(m => m.IsSupplyLow())
            .OrderBy(m => m.CurrentSupply)
            .ToList();
    }

    /// <summary>
    /// Gets medications that have expired.
    /// </summary>
    public async Task<List<Medication>> GetExpiredMedicationsAsync(int? personId = null)
    {
        await InitializeAsync();

        var medications = await GetMedicationsAsync(personId, includeDeleted: false);

        return medications
            .Where(m => m.IsExpired())
            .OrderBy(m => m.ExpirationDate)
            .ToList();
    }

    /// <summary>
    /// Searches for medications by name.
    /// Returns multiple rows matching the search criteria.
    /// Demonstrates Task 3 search functionality.
    /// </summary>
    public async Task<List<Medication>> SearchMedicationsAsync(string searchTerm, int? personId = null)
    {
        await InitializeAsync();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetMedicationsAsync(personId);
        }

        // Sanitize input before using it in search logic
        searchTerm = ValidationHelper.SanitizeInput(searchTerm);
        var term = searchTerm.Trim().ToLowerInvariant();

        var allMedications = personId.HasValue
            ? await _db.Table<Medication>()
                .Where(m => m.PersonId == personId.Value && !m.IsDeleted)
                .ToListAsync()
            : await _db.Table<Medication>()
                .Where(m => !m.IsDeleted)
                .ToListAsync();

        return allMedications
            .Where(m =>
                (m.Name ?? string.Empty).ToLowerInvariant().Contains(term) ||
                (m.Dosage ?? string.Empty).ToLowerInvariant().Contains(term) ||
                (m.PrescribingDoctor ?? string.Empty).ToLowerInvariant().Contains(term) ||
                (m.Instructions ?? string.Empty).ToLowerInvariant().Contains(term))
            .OrderBy(m => m.Name)
            .ToList();
    }

    // ==================== MEDICATION: CREATE / UPDATE ====================

    /// <summary>
    /// Saves (inserts or updates) a medication.
    /// Validates the entity before saving.
    /// </summary>
    public async Task<int> SaveMedicationAsync(Medication medication)
    {
        if (medication is null)
        {
            throw new ArgumentNullException(nameof(medication));
        }

        await InitializeAsync();

        if (!medication.Validate())
        {
            throw new InvalidOperationException("Medication entity is not valid.");
        }

        medication.MarkAsUpdated();

        if (medication.Id != 0)
        {
            return await _db.UpdateAsync(medication);
        }

        return await _db.InsertAsync(medication);
    }

    // ==================== MEDICATION: DELETE ====================

    /// <summary>
    /// Soft-deletes the medication (marks IsDeleted) and updates the record.
    /// Preserves data for audit trail.
    /// </summary>
    public async Task<int> SoftDeleteMedicationAsync(Medication medication)
    {
        if (medication is null)
        {
            throw new ArgumentNullException(nameof(medication));
        }

        await InitializeAsync();

        medication.SoftDelete();
        return await _db.UpdateAsync(medication);
    }

    /// <summary>
    /// Hard delete from database (permanently removes the record).
    /// </summary>
    public async Task<int> HardDeleteMedicationAsync(Medication medication)
    {
        if (medication is null)
        {
            throw new ArgumentNullException(nameof(medication));
        }

        await InitializeAsync();

        return await _db.DeleteAsync(medication);
    }

    // ==================== UTILITY METHODS ====================

    /// <summary>
    /// Gets the count of medications for a specific person.
    /// </summary>
    public async Task<int> GetMedicationCountForPersonAsync(int personId)
    {
        await InitializeAsync();

        return await _db.Table<Medication>()
            .Where(m => m.PersonId == personId && !m.IsDeleted)
            .CountAsync();
    }

    /// <summary>
    /// Gets the count of active medications for a specific person.
    /// </summary>
    public async Task<int> GetActiveMedicationCountForPersonAsync(int personId)
    {
        await InitializeAsync();

        return await _db.Table<Medication>()
            .Where(m => m.PersonId == personId && m.IsActive && !m.IsDeleted)
            .CountAsync();
    }

    // ==================== SCHEDULE RULE: READ ====================

    /// <summary>
    /// Gets all schedule rules for a specific medication.
    /// </summary>
    public async Task<List<ScheduleRule>> GetSchedulesForMedicationAsync(int medicationId)
    {
        await InitializeAsync();

        var schedules = new List<ScheduleRule>();

        // Get all schedule types
        var dailySchedules = await _db.Table<DailySchedule>()
            .Where(s => s.MedicationId == medicationId && !s.IsDeleted)
            .ToListAsync();
        schedules.AddRange(dailySchedules);

        var intervalSchedules = await _db.Table<IntervalSchedule>()
            .Where(s => s.MedicationId == medicationId && !s.IsDeleted)
            .ToListAsync();
        schedules.AddRange(intervalSchedules);

        var weeklySchedules = await _db.Table<WeeklySchedule>()
            .Where(s => s.MedicationId == medicationId && !s.IsDeleted)
            .ToListAsync();
        schedules.AddRange(weeklySchedules);

        var asNeededSchedules = await _db.Table<AsNeededSchedule>()
            .Where(s => s.MedicationId == medicationId && !s.IsDeleted)
            .ToListAsync();
        schedules.AddRange(asNeededSchedules);

        return schedules.OrderBy(s => s.CreatedAt).ToList();
    }

    /// <summary>
    /// Gets all active schedule rules.
    /// </summary>
    public async Task<List<ScheduleRule>> GetActiveSchedulesAsync()
    {
        try
        {
            await InitializeAsync();

            var schedules = new List<ScheduleRule>();

            // Get all schedule types (polymorphic query)
            var dailySchedules = await _db.Table<DailySchedule>()
                .Where(s => s.IsActive && !s.IsDeleted)
                .ToListAsync();
            schedules.AddRange(dailySchedules);

            var intervalSchedules = await _db.Table<IntervalSchedule>()
                .Where(s => s.IsActive && !s.IsDeleted)
                .ToListAsync();
            schedules.AddRange(intervalSchedules);

            var weeklySchedules = await _db.Table<WeeklySchedule>()
                .Where(s => s.IsActive && !s.IsDeleted)
                .ToListAsync();
            schedules.AddRange(weeklySchedules);

            var asNeededSchedules = await _db.Table<AsNeededSchedule>()
                .Where(s => s.IsActive && !s.IsDeleted)
                .ToListAsync();
            schedules.AddRange(asNeededSchedules);

            return schedules;
        }
        catch (Exception)
        {
            // If schedule tables don't exist yet or error occurs, return empty list
            // This prevents crashes when schedules aren't initialized
            return new List<ScheduleRule>();
        }
    }

    // ==================== SCHEDULE RULE: CREATE / UPDATE ====================

    /// <summary>
    /// Saves (inserts or updates) a schedule rule.
    /// Uses polymorphism to save the appropriate concrete type.
    /// </summary>
    public async Task<int> SaveScheduleAsync(ScheduleRule schedule)
    {
        if (schedule is null)
        {
            throw new ArgumentNullException(nameof(schedule));
        }

        await InitializeAsync();

        if (!schedule.Validate())
        {
            throw new InvalidOperationException("Schedule entity is not valid.");
        }

        schedule.MarkAsUpdated();

        // Save based on concrete type - demonstrates polymorphism
        return schedule switch
        {
            DailySchedule daily => daily.Id != 0 
                ? await _db.UpdateAsync(daily) 
                : await _db.InsertAsync(daily),
            IntervalSchedule interval => interval.Id != 0 
                ? await _db.UpdateAsync(interval) 
                : await _db.InsertAsync(interval),
            WeeklySchedule weekly => weekly.Id != 0 
                ? await _db.UpdateAsync(weekly) 
                : await _db.InsertAsync(weekly),
            AsNeededSchedule asNeeded => asNeeded.Id != 0 
                ? await _db.UpdateAsync(asNeeded) 
                : await _db.InsertAsync(asNeeded),
            _ => throw new ArgumentException($"Unknown schedule type: {schedule.GetType().Name}", nameof(schedule))
        };
    }

    // ==================== SCHEDULE RULE: DELETE ====================

    /// <summary>
    /// Soft-deletes a schedule rule.
    /// </summary>
    public async Task<int> SoftDeleteScheduleAsync(ScheduleRule schedule)
    {
        if (schedule is null)
        {
            throw new ArgumentNullException(nameof(schedule));
        }

        await InitializeAsync();

        schedule.SoftDelete();
        
        // Update based on concrete type
        return schedule switch
        {
            DailySchedule daily => await _db.UpdateAsync(daily),
            IntervalSchedule interval => await _db.UpdateAsync(interval),
            WeeklySchedule weekly => await _db.UpdateAsync(weekly),
            AsNeededSchedule asNeeded => await _db.UpdateAsync(asNeeded),
            _ => throw new ArgumentException($"Unknown schedule type: {schedule.GetType().Name}", nameof(schedule))
        };
    }
}
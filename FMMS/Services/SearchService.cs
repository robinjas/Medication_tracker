using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FMMS.Models;

namespace FMMS.Services;

/// <summary>
/// Unified search service that searches across all entity types in the FMMS application.
/// Returns SearchResult objects that can be displayed in a unified search results view.
/// Demonstrates Task 3 requirement for search functionality with multiple row results.
/// </summary>
public class SearchService
{
    private readonly DatabaseService _database;

    public SearchService(DatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>
    /// Searches across all entities (People, Medications, and Schedules) and returns unified results.
    /// Returns multiple rows from different entity types matching the search criteria.
    /// Demonstrates search across polymorphic entities.
    /// </summary>
    /// <param name="searchTerm">The search keyword</param>
    /// <returns>List of SearchResult objects from multiple entity types</returns>
    public async Task<List<SearchResult>> SearchAllAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<SearchResult>();
        }

        var results = new List<SearchResult>();

        // Search people
        var people = await _database.SearchPeopleAsync(searchTerm);
        results.AddRange(people.Select(p => new SearchResult
        {
            EntityType = "Person",
            Title = p.ToString(),
            Subtitle = p.DateOfBirth.HasValue 
                ? $"DOB: {p.DateOfBirth.Value:yyyy-MM-dd}" 
                : "Family Member",
            Details = $"ID: {p.Id}",
            EntityId = p.Id,
            Entity = p,
            Timestamp = p.UpdatedAt
        }));

        // Search medications
        var medications = await _database.SearchMedicationsAsync(searchTerm);
        results.AddRange(medications.Select(m => new SearchResult
        {
            EntityType = "Medication",
            Title = m.Name,
            Subtitle = !string.IsNullOrWhiteSpace(m.Dosage) ? m.Dosage : "Medication",
            Details = $"Supply: {m.CurrentSupply} doses | Prescribed by: {m.PrescribingDoctor}",
            EntityId = m.Id,
            PersonId = m.PersonId,
            Entity = m,
            Timestamp = m.UpdatedAt
        }));

        // Search schedules (polymorphic search across all schedule types)
        var schedules = await SearchSchedulesAsync(searchTerm);
        results.AddRange(schedules);

        // Sort by entity type, then by title
        return results
            .OrderBy(r => r.EntityType)
            .ThenBy(r => r.Title)
            .ToList();
    }

    /// <summary>
    /// Searches across all schedule types (polymorphic search).
    /// Demonstrates searching across polymorphic entities.
    /// </summary>
    private async Task<List<SearchResult>> SearchSchedulesAsync(string searchTerm)
    {
        var results = new List<SearchResult>();
        var term = searchTerm.Trim().ToLowerInvariant();

        try
        {
            // Get all active schedules (polymorphic collection)
            var allSchedules = await _database.GetActiveSchedulesAsync();

            // Search across all schedule types using polymorphic methods
            var matchingSchedules = allSchedules.Where(s =>
                s.ScheduleType.ToLowerInvariant().Contains(term) ||
                s.GetScheduleDescription().ToLowerInvariant().Contains(term) ||
                (s.Notes ?? string.Empty).ToLowerInvariant().Contains(term)
            ).ToList();

            results.AddRange(matchingSchedules.Select(s => new SearchResult
            {
                EntityType = $"Schedule ({s.ScheduleType})",
                Title = s.GetScheduleDescription(), // Polymorphic method call
                Subtitle = $"Medication ID: {s.MedicationId}",
                Details = $"Type: {s.ScheduleType} | Active: {(s.IsActive ? "Yes" : "No")}",
                EntityId = s.Id,
                Entity = s,
                Timestamp = s.UpdatedAt
            }));
        }
        catch (Exception)
        {
            // If schedules aren't initialized yet or error occurs, return empty list
            // This prevents the app from crashing when schedule tables don't exist
        }

        return results;
    }

    /// <summary>
    /// Searches medications only, with optional person filter.
    /// Returns multiple medication rows matching the search criteria.
    /// </summary>
    public async Task<List<SearchResult>> SearchMedicationsAsync(string searchTerm, int? personId = null)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<SearchResult>();
        }

        var medications = await _database.SearchMedicationsAsync(searchTerm, personId);
        
        return medications.Select(m => new SearchResult
        {
            EntityType = "Medication",
            Title = m.Name,
            Subtitle = !string.IsNullOrWhiteSpace(m.Dosage) ? m.Dosage : "Medication",
            Details = $"Supply: {m.CurrentSupply} doses | Prescribed by: {m.PrescribingDoctor}",
            EntityId = m.Id,
            PersonId = m.PersonId,
            Entity = m,
            Timestamp = m.UpdatedAt
        })
        .OrderBy(r => r.Title)
        .ToList();
    }

    /// <summary>
    /// Searches people only.
    /// Returns multiple person rows matching the search criteria.
    /// </summary>
    public async Task<List<SearchResult>> SearchPeopleAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<SearchResult>();
        }

        var people = await _database.SearchPeopleAsync(searchTerm);
        
        return people.Select(p => new SearchResult
        {
            EntityType = "Person",
            Title = p.ToString(),
            Subtitle = p.DateOfBirth.HasValue 
                ? $"DOB: {p.DateOfBirth.Value:yyyy-MM-dd}" 
                : "Family Member",
            Details = $"ID: {p.Id}",
            EntityId = p.Id,
            Entity = p,
            Timestamp = p.UpdatedAt
        })
        .OrderBy(r => r.Title)
        .ToList();
    }

    /// <summary>
    /// Searches by date range across medications (prescription date, expiration date).
    /// Returns multiple rows matching the date criteria.
    /// </summary>
    public async Task<List<SearchResult>> SearchByDateAsync(DateTime? startDate, DateTime? endDate)
    {
        var results = new List<SearchResult>();

        if (!startDate.HasValue && !endDate.HasValue)
        {
            return results;
        }

        // Get all medications
        var allMedications = await _database.GetMedicationsAsync(includeDeleted: false);

        // Filter by date range
        var filteredMedications = allMedications.Where(m =>
        {
            bool matchesStart = !startDate.HasValue || 
                (m.PrescriptionDate.HasValue && m.PrescriptionDate.Value >= startDate.Value) ||
                (m.ExpirationDate.HasValue && m.ExpirationDate.Value >= startDate.Value);

            bool matchesEnd = !endDate.HasValue ||
                (m.PrescriptionDate.HasValue && m.PrescriptionDate.Value <= endDate.Value) ||
                (m.ExpirationDate.HasValue && m.ExpirationDate.Value <= endDate.Value);

            return matchesStart && matchesEnd;
        });

        results.AddRange(filteredMedications.Select(m => new SearchResult
        {
            EntityType = "Medication",
            Title = m.Name,
            Subtitle = !string.IsNullOrWhiteSpace(m.Dosage) ? m.Dosage : "Medication",
            Details = $"Prescribed: {m.PrescriptionDate:yyyy-MM-dd} | Expires: {m.ExpirationDate:yyyy-MM-dd}",
            EntityId = m.Id,
            PersonId = m.PersonId,
            Entity = m,
            Timestamp = m.PrescriptionDate ?? m.UpdatedAt
        }));

        return results
            .OrderBy(r => r.Timestamp)
            .ToList();
    }

    /// <summary>
    /// Advanced medication search with multiple criteria.
    /// Returns multiple medication rows matching all specified criteria.
    /// </summary>
    public async Task<List<SearchResult>> SearchMedicationsAdvancedAsync(
        string? name = null,
        string? dosage = null,
        string? prescriber = null,
        int? personId = null,
        bool? isActive = null)
    {
        var allMedications = await _database.GetMedicationsAsync(personId, includeDeleted: false);

        var filtered = allMedications.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var nameTerm = name.Trim().ToLowerInvariant();
            filtered = filtered.Where(m => 
                (m.Name ?? string.Empty).ToLowerInvariant().Contains(nameTerm));
        }

        if (!string.IsNullOrWhiteSpace(dosage))
        {
            var dosageTerm = dosage.Trim().ToLowerInvariant();
            filtered = filtered.Where(m => 
                (m.Dosage ?? string.Empty).ToLowerInvariant().Contains(dosageTerm));
        }

        if (!string.IsNullOrWhiteSpace(prescriber))
        {
            var prescriberTerm = prescriber.Trim().ToLowerInvariant();
            filtered = filtered.Where(m => 
                (m.PrescribingDoctor ?? string.Empty).ToLowerInvariant().Contains(prescriberTerm));
        }

        if (isActive.HasValue)
        {
            filtered = filtered.Where(m => m.IsActive == isActive.Value);
        }

        var medications = filtered.ToList();

        return medications.Select(m => new SearchResult
        {
            EntityType = "Medication",
            Title = m.Name,
            Subtitle = !string.IsNullOrWhiteSpace(m.Dosage) ? m.Dosage : "Medication",
            Details = $"Supply: {m.CurrentSupply} | Prescribed by: {m.PrescribingDoctor} | Active: {(m.IsActive ? "Yes" : "No")}",
            EntityId = m.Id,
            PersonId = m.PersonId,
            Entity = m,
            Timestamp = m.UpdatedAt
        })
        .OrderBy(r => r.Title)
        .ToList();
    }
}


using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMMS.Services;
using Microsoft.Maui.Storage;

namespace FMMS.Reports;

/// <summary>
/// Handles report generation for the FMMS app.
/// Generates CSV reports with title, timestamp, multiple columns and rows.
/// </summary>
public class ReportService
{
    private readonly DatabaseService _database;

    public ReportService(DatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>
    /// Returns the directory where reports are stored, creating it if needed.
    /// </summary>
    public static string GetReportsDirectory()
    {
        var baseDir = FileSystem.AppDataDirectory;
        var reportsDir = Path.Combine(baseDir, "Reports");

        if (!Directory.Exists(reportsDir))
        {
            Directory.CreateDirectory(reportsDir);
        }

        return reportsDir;
    }

    /// <summary>
    /// Generates a filename like PeopleSummary_2025-11-23_153012.csv
    /// </summary>
    public static string GenerateReportFilename(string baseName)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss");
        return $"{baseName}_{timestamp}.csv";
    }

    /// <summary>
    /// Generates a people summary CSV report with:
    /// - Title
    /// - Generated timestamp
    /// - Multiple columns and rows
    /// </summary>
    public async Task GeneratePeopleSummaryReportAsync(string outputPath)
    {
        var people = await _database.GetPeopleAsync(includeDeleted: true);

        var sb = new StringBuilder();

        // Title + timestamp
        sb.AppendLine("Family Members Summary Report");
        sb.AppendLine($"Generated At (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // Header row (multiple columns)
        sb.AppendLine("Id,FirstName,LastName,CreatedAtUtc,UpdatedAtUtc,IsDeleted");

        // Data rows
        foreach (var person in people)
        {
            var line = string.Join(",",
                person.Id,
                EscapeCsv(person.FirstName),
                EscapeCsv(person.LastName),
                person.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                person.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                person.IsDeleted ? "Yes" : "No");

            sb.AppendLine(line);
        }

        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// Generates a comprehensive medication summary report for all people:
    /// - Title
    /// - Generated timestamp
    /// - Multiple columns and rows
    /// </summary>
    public async Task GenerateMedicationSummaryReportAsync(string outputPath)
    {
        var people = await _database.GetPeopleAsync(includeDeleted: false);
        var allMedications = await _database.GetMedicationsAsync(includeDeleted: false);

        var sb = new StringBuilder();

        // Title + timestamp
        sb.AppendLine("Comprehensive Medication Summary Report");
        sb.AppendLine($"Generated At (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // Header row (multiple columns)
        sb.AppendLine("PersonId,PersonName,MedicationId,MedicationName,Dosage,CurrentSupply,LowThreshold,RefillsRemaining,IsActive,IsLowSupply,IsExpired,Pharmacy");

        // Data rows - group by person
        foreach (var person in people)
        {
            var personMedications = allMedications.Where(m => m.PersonId == person.Id);
            
            foreach (var medication in personMedications)
            {
                var line = string.Join(",",
                    person.Id,
                    EscapeCsv(person.ToString()),
                    medication.Id,
                    EscapeCsv(medication.Name),
                    EscapeCsv(medication.Dosage),
                    medication.CurrentSupply,
                    medication.LowSupplyThreshold,
                    medication.RefillsRemaining,
                    medication.IsActive ? "Yes" : "No",
                    medication.IsSupplyLow() ? "Yes" : "No",
                    medication.IsExpired() ? "Yes" : "No",
                    EscapeCsv(medication.Pharmacy));

                sb.AppendLine(line);
            }
        }

        // Summary statistics
        sb.AppendLine();
        sb.AppendLine("Summary Statistics");
        sb.AppendLine($"Total People: {people.Count}");
        sb.AppendLine($"Total Medications: {allMedications.Count}");
        sb.AppendLine($"Active Medications: {allMedications.Count(m => m.IsActive)}");
        sb.AppendLine($"Low Supply Medications: {allMedications.Count(m => m.IsSupplyLow())}");
        sb.AppendLine($"Expired Medications: {allMedications.Count(m => m.IsExpired())}");
        sb.AppendLine($"Medications Needing Refill: {allMedications.Count(m => m.NeedsRefill())}");

        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    private static string EscapeCsv(string? value)
    {
        value ??= string.Empty;

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }

        return value;
    }
}

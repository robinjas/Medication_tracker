using System;

namespace FMMS.Models;

/// <summary>
/// Abstract base class for medication schedule rules.
/// Demonstrates inheritance (from BaseEntity) and polymorphism through abstract methods.
/// Each concrete schedule type implements the abstract methods differently, showing polymorphic behavior.
/// </summary>
public abstract class ScheduleRule : BaseEntity
{
    /// <summary>
    /// The ID of the medication this schedule belongs to.
    /// Foreign key to Medication.Id
    /// </summary>
    public int MedicationId { get; set; }

    /// <summary>
    /// The type of schedule (Daily, Interval, Weekly, AsNeeded).
    /// Used by the factory pattern to create appropriate subtype.
    /// </summary>
    public string ScheduleType { get; set; } = string.Empty;

    /// <summary>
    /// Optional description or notes for this schedule.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Indicates if this schedule is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Abstract method to calculate the next dose time from a given date.
    /// Each schedule type implements this differently, demonstrating polymorphism.
    /// </summary>
    /// <param name="fromDate">The date to calculate from (usually current time)</param>
    /// <returns>The next scheduled dose time, or null if no future doses</returns>
    public abstract DateTime? CalculateNextDose(DateTime fromDate);

    /// <summary>
    /// Abstract method to check if a medication is due at a specific time.
    /// Each schedule type implements this differently, demonstrating polymorphism.
    /// </summary>
    /// <param name="checkTime">The time to check</param>
    /// <returns>True if medication is due at the specified time</returns>
    public abstract bool IsDueAt(DateTime checkTime);

    /// <summary>
    /// Abstract method to get a human-readable description of the schedule.
    /// Each schedule type returns a different description, demonstrating polymorphism.
    /// </summary>
    /// <returns>A string describing the schedule pattern</returns>
    public abstract string GetScheduleDescription();

    /// <summary>
    /// Factory pattern method to create an appropriate ScheduleRule subtype.
    /// Demonstrates polymorphism by creating different concrete types based on the schedule type.
    /// </summary>
    /// <param name="scheduleType">The type of schedule: "Daily", "Interval", "Weekly", "AsNeeded"</param>
    /// <param name="medicationId">The ID of the medication this schedule belongs to</param>
    /// <returns>A new instance of the appropriate ScheduleRule subtype</returns>
    public static ScheduleRule CreateSchedule(string scheduleType, int medicationId)
    {
        return scheduleType.ToLowerInvariant() switch
        {
            "daily" => new DailySchedule { MedicationId = medicationId, ScheduleType = "Daily" },
            "interval" => new IntervalSchedule { MedicationId = medicationId, ScheduleType = "Interval" },
            "weekly" => new WeeklySchedule { MedicationId = medicationId, ScheduleType = "Weekly" },
            "asneeded" or "as-needed" or "prn" => new AsNeededSchedule { MedicationId = medicationId, ScheduleType = "AsNeeded" },
            _ => throw new ArgumentException($"Unknown schedule type: {scheduleType}", nameof(scheduleType))
        };
    }

    /// <summary>
    /// Validates the schedule rule.
    /// </summary>
    public override bool Validate()
    {
        if (!base.Validate())
        {
            return false;
        }

        if (MedicationId <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(ScheduleType))
        {
            return false;
        }

        return true;
    }
}


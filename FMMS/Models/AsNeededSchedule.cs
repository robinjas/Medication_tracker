using System;

namespace FMMS.Models;

/// <summary>
/// Represents an "as needed" (PRN) medication schedule.
/// Medication can be taken when needed, with optional minimum interval between doses.
/// Demonstrates polymorphism by implementing ScheduleRule abstract methods differently from other schedule types.
/// </summary>
public class AsNeededSchedule : ScheduleRule
{
    /// <summary>
    /// Minimum hours between doses (to prevent overdose).
    /// If 0, medication can be taken at any time.
    /// </summary>
    public int MinimumHoursBetweenDoses { get; set; } = 0;

    /// <summary>
    /// The last time a dose was taken (for tracking minimum interval).
    /// </summary>
    public DateTime? LastDoseTime { get; set; }

    /// <summary>
    /// For as-needed schedules, there's no "next dose" - it's taken when needed.
    /// Returns null to indicate no scheduled time.
    /// </summary>
    public override DateTime? CalculateNextDose(DateTime fromDate)
    {
        if (!IsActive)
        {
            return null;
        }

        // As-needed medications don't have a scheduled next dose
        // But if there's a minimum interval, we can indicate when it's safe to take again
        if (MinimumHoursBetweenDoses > 0 && LastDoseTime.HasValue)
        {
            var nextAllowedTime = LastDoseTime.Value.AddHours(MinimumHoursBetweenDoses);
            if (nextAllowedTime > fromDate)
            {
                return nextAllowedTime; // Return when it's safe to take again
            }
        }

        return null; // Can be taken anytime (or already allowed)
    }

    /// <summary>
    /// Checks if medication can be taken at the specified time.
    /// For as-needed, this checks if minimum interval has passed (if specified).
    /// </summary>
    public override bool IsDueAt(DateTime checkTime)
    {
        if (!IsActive)
        {
            return false;
        }

        // As-needed medications are always "due" if minimum interval has passed
        if (MinimumHoursBetweenDoses == 0 || !LastDoseTime.HasValue)
        {
            return true; // Can be taken anytime
        }

        var timeSinceLastDose = (checkTime - LastDoseTime.Value).TotalHours;
        return timeSinceLastDose >= MinimumHoursBetweenDoses;
    }

    /// <summary>
    /// Returns a human-readable description of the as-needed schedule.
    /// </summary>
    public override string GetScheduleDescription()
    {
        if (MinimumHoursBetweenDoses > 0)
        {
            return $"As needed (minimum {MinimumHoursBetweenDoses} hours between doses)";
        }

        return "As needed";
    }

    /// <summary>
    /// Records that a dose was taken at the specified time.
    /// This updates LastDoseTime for tracking minimum intervals.
    /// </summary>
    public void RecordDoseTaken(DateTime takenTime)
    {
        LastDoseTime = takenTime;
        MarkAsUpdated();
    }
}


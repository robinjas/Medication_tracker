using System;

namespace FMMS.Models;

/// <summary>
/// Represents an interval-based medication schedule.
/// Medication is taken every X hours or every X days.
/// Demonstrates polymorphism by implementing ScheduleRule abstract methods differently from DailySchedule.
/// </summary>
public class IntervalSchedule : ScheduleRule
{
    /// <summary>
    /// The interval amount (e.g., 4 for "every 4 hours" or 2 for "every 2 days").
    /// </summary>
    public int IntervalAmount { get; set; } = 1;

    /// <summary>
    /// The interval unit: "Hours" or "Days".
    /// </summary>
    public string IntervalUnit { get; set; } = "Hours";

    /// <summary>
    /// The start time for the interval schedule.
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Calculates the next dose time for an interval schedule.
    /// Based on the start time and interval, calculates when the next dose is due.
    /// </summary>
    public override DateTime? CalculateNextDose(DateTime fromDate)
    {
        if (!IsActive)
        {
            return null;
        }

        if (IntervalAmount <= 0)
        {
            return null;
        }

        var nextDose = StartTime;

        // Calculate next dose based on interval
        while (nextDose <= fromDate)
        {
            if (IntervalUnit.Equals("Hours", StringComparison.OrdinalIgnoreCase))
            {
                nextDose = nextDose.AddHours(IntervalAmount);
            }
            else if (IntervalUnit.Equals("Days", StringComparison.OrdinalIgnoreCase))
            {
                nextDose = nextDose.AddDays(IntervalAmount);
            }
            else
            {
                return null; // Invalid unit
            }
        }

        return nextDose;
    }

    /// <summary>
    /// Checks if medication is due at the specified time (within a tolerance window).
    /// </summary>
    public override bool IsDueAt(DateTime checkTime)
    {
        if (!IsActive)
        {
            return false;
        }

        if (checkTime < StartTime)
        {
            return false;
        }

        var nextDose = CalculateNextDose(checkTime);
        if (!nextDose.HasValue)
        {
            return false;
        }

        // Consider due if within 30 minutes of scheduled time
        var timeDifference = Math.Abs((checkTime - nextDose.Value).TotalMinutes);
        return timeDifference <= 30;
    }

    /// <summary>
    /// Returns a human-readable description of the interval schedule.
    /// </summary>
    public override string GetScheduleDescription()
    {
        var unit = IntervalUnit.Equals("Hours", StringComparison.OrdinalIgnoreCase) ? "hour(s)" : "day(s)";
        return $"Every {IntervalAmount} {unit}";
    }
}


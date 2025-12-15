using System;
using System.Linq;

namespace FMMS.Models;

/// <summary>
/// Represents a weekly medication schedule.
/// Medication is taken on specific days of the week.
/// Demonstrates polymorphism by implementing ScheduleRule abstract methods differently from other schedule types.
/// </summary>
public class WeeklySchedule : ScheduleRule
{
    /// <summary>
    /// The days of the week when medication should be taken.
    /// Comma-separated day names: "Monday,Wednesday,Friday" or bit flags.
    /// </summary>
    public string DaysOfWeek { get; set; } = string.Empty;

    /// <summary>
    /// The time of day to take the medication (in minutes since midnight).
    /// </summary>
    public int TimeOfDay { get; set; } = 480; // Default: 8:00 AM

    /// <summary>
    /// Calculates the next dose time for a weekly schedule.
    /// Finds the next occurrence of one of the specified days at the specified time.
    /// </summary>
    public override DateTime? CalculateNextDose(DateTime fromDate)
    {
        if (!IsActive)
        {
            return null;
        }

        var days = ParseDaysOfWeek();
        if (days.Count == 0)
        {
            return null;
        }

        var checkDate = fromDate.Date;
        var currentMinutes = (int)fromDate.TimeOfDay.TotalMinutes;
        var scheduledTime = checkDate.AddMinutes(TimeOfDay);

        // Check today first
        if (days.Contains(checkDate.DayOfWeek))
        {
            if (scheduledTime > fromDate)
            {
                return scheduledTime;
            }
        }

        // Check next 7 days
        for (int i = 1; i <= 7; i++)
        {
            checkDate = checkDate.AddDays(1);
            scheduledTime = checkDate.AddMinutes(TimeOfDay);

            if (days.Contains(checkDate.DayOfWeek))
            {
                return scheduledTime;
            }
        }

        return null; // Should never reach here, but just in case
    }

    /// <summary>
    /// Checks if medication is due at the specified time (on a scheduled day, within time window).
    /// </summary>
    public override bool IsDueAt(DateTime checkTime)
    {
        if (!IsActive)
        {
            return false;
        }

        var days = ParseDaysOfWeek();
        if (!days.Contains(checkTime.DayOfWeek))
        {
            return false;
        }

        var checkMinutes = (int)checkTime.TimeOfDay.TotalMinutes;
        // Consider due if within 15 minutes of scheduled time
        return Math.Abs(checkMinutes - TimeOfDay) <= 15;
    }

    /// <summary>
    /// Returns a human-readable description of the weekly schedule.
    /// </summary>
    public override string GetScheduleDescription()
    {
        var days = ParseDaysOfWeek();
        if (days.Count == 0)
        {
            return "Weekly (no days specified)";
        }

        var dayNames = days.Select(d => d.ToString()).ToList();

        var hours = TimeOfDay / 60;
        var minutes = TimeOfDay % 60;
        var ampm = hours >= 12 ? "PM" : "AM";
        var displayHours = hours > 12 ? hours - 12 : (hours == 0 ? 12 : hours);
        var timeString = $"{displayHours}:{minutes:D2} {ampm}";

        if (dayNames.Count == 7)
        {
            return $"Daily at {timeString}";
        }

        return $"Weekly on {string.Join(", ", dayNames)} at {timeString}";
    }

    private List<DayOfWeek> ParseDaysOfWeek()
    {
        var days = new List<DayOfWeek>();
        if (string.IsNullOrWhiteSpace(DaysOfWeek))
        {
            return days;
        }

        var parts = DaysOfWeek.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            if (Enum.TryParse<DayOfWeek>(part, true, out DayOfWeek day))
            {
                if (!days.Contains(day))
                {
                    days.Add(day);
                }
            }
        }

        return days;
    }
}


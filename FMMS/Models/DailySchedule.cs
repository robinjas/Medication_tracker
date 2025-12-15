using System;
using System.Linq;

namespace FMMS.Models;

/// <summary>
/// Represents a daily medication schedule.
/// Medication is taken every day at specified times.
/// Demonstrates polymorphism by implementing ScheduleRule abstract methods.
/// </summary>
public class DailySchedule : ScheduleRule
{
    /// <summary>
    /// The time(s) of day to take the medication (in 24-hour format, as minutes since midnight).
    /// For example: [480] = 8:00 AM, [1320] = 10:00 PM, [480, 1320] = 8:00 AM and 10:00 PM
    /// </summary>
    public string TimesOfDay { get; set; } = string.Empty; // JSON array or comma-separated minutes

    /// <summary>
    /// Calculates the next dose time for a daily schedule.
    /// Returns the next scheduled time today, or the first time tomorrow if all today's times have passed.
    /// </summary>
    public override DateTime? CalculateNextDose(DateTime fromDate)
    {
        if (!IsActive)
        {
            return null;
        }

        var times = ParseTimes();
        if (times.Count == 0)
        {
            return null;
        }

        var today = fromDate.Date;
        var currentMinutes = (int)fromDate.TimeOfDay.TotalMinutes;

        // Find next time today
        foreach (var time in times)
        {
            var doseTime = today.AddMinutes(time);
            if (doseTime > fromDate)
            {
                return doseTime;
            }
        }

        // All times today have passed, return first time tomorrow
        return today.AddDays(1).AddMinutes(times[0]);
    }

    /// <summary>
    /// Checks if medication is due at the specified time (within a 15-minute window).
    /// </summary>
    public override bool IsDueAt(DateTime checkTime)
    {
        if (!IsActive)
        {
            return false;
        }

        var times = ParseTimes();
        var checkMinutes = (int)checkTime.TimeOfDay.TotalMinutes;

        foreach (var time in times)
        {
            // Consider due if within 15 minutes before or after scheduled time
            if (Math.Abs(checkMinutes - time) <= 15)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns a human-readable description of the daily schedule.
    /// </summary>
    public override string GetScheduleDescription()
    {
        var times = ParseTimes();
        if (times.Count == 0)
        {
            return "Daily (no times specified)";
        }

        var timeStrings = times.Select(t =>
        {
            var hours = t / 60;
            var minutes = t % 60;
            var ampm = hours >= 12 ? "PM" : "AM";
            var displayHours = hours > 12 ? hours - 12 : (hours == 0 ? 12 : hours);
            return $"{displayHours}:{minutes:D2} {ampm}";
        }).ToList();

        if (timeStrings.Count == 1)
        {
            return $"Daily at {timeStrings[0]}";
        }

        return $"Daily at {string.Join(", ", timeStrings)}";
    }

    private List<int> ParseTimes()
    {
        var times = new List<int>();
        if (string.IsNullOrWhiteSpace(TimesOfDay))
        {
            return times;
        }

        // Try to parse as comma-separated values
        var parts = TimesOfDay.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            if (int.TryParse(part, out int minutes) && minutes >= 0 && minutes < 1440)
            {
                times.Add(minutes);
            }
        }

        return times.OrderBy(t => t).ToList();
    }
}


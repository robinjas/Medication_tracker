using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FMMS.Models;
using Microsoft.Maui.Controls;

namespace FMMS.Services;

/// <summary>
/// Service for managing medication reminders and notifications.
/// Checks for due medications periodically and sends alerts.
/// </summary>
public class NotificationService : IDisposable
{
    private readonly DatabaseService _database;
    private Timer? _checkTimer;
    private bool _isRunning;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute
    private DateTime _lastCheckTime = DateTime.MinValue;
    
    // Track which medications have been notified today to prevent duplicate alerts
    // Key: medicationId_date (e.g., "123_2024-01-15" for medication 123 on Jan 15)
    // Value: List of scheduled times (in minutes) that were already notified today
    private readonly Dictionary<string, HashSet<int>> _notifiedToday = new();

    public NotificationService(DatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>
    /// Starts the notification service to periodically check for due medications.
    /// </summary>
    public void Start()
    {
        if (_isRunning)
        {
            return;
        }

        _isRunning = true;
        _checkTimer = new Timer(CheckForDueMedications, null, TimeSpan.Zero, _checkInterval);
    }

    /// <summary>
    /// Stops the notification service.
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _checkTimer?.Dispose();
        _checkTimer = null;
    }

    /// <summary>
    /// Immediately checks for due medications and sends notifications.
    /// </summary>
    public async Task CheckNowAsync()
    {
        await CheckForDueMedicationsAsync();
    }

    /// <summary>
    /// Timer callback to check for due medications.
    /// </summary>
    private async void CheckForDueMedications(object? state)
    {
        if (!_isRunning)
        {
            return;
        }

        await CheckForDueMedicationsAsync();
    }

    /// <summary>
    /// Checks all active medications with schedules and sends notifications for due ones.
    /// </summary>
    private async Task CheckForDueMedicationsAsync()
    {
        try
        {
            var now = DateTime.Now;
            
            // Prevent checking too frequently (at least 30 seconds between checks)
            if ((now - _lastCheckTime).TotalSeconds < 30)
            {
                return;
            }

            _lastCheckTime = now;

            // Get all active medications
            var medications = await _database.GetMedicationsAsync();
            var activeMedications = medications.Where(m => m.IsActive && !m.IsDeleted).ToList();

            if (activeMedications.Count == 0)
            {
                return;
            }

            // Get all active schedules
            var schedules = await _database.GetActiveSchedulesAsync();
            
            // Group schedules by medication ID
            var schedulesByMedication = schedules
                .GroupBy(s => s.MedicationId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var dueMedications = new List<(Medication medication, int scheduledTimeMinutes)>();

            // Check each medication for due schedules
            foreach (var medication in activeMedications)
            {
                if (!schedulesByMedication.TryGetValue(medication.Id, out var medicationSchedules))
                {
                    continue;
                }

                // Check each schedule for this medication
                foreach (var schedule in medicationSchedules)
                {
                    if (schedule.IsDueAt(now))
                    {
                        // For daily schedules, find which specific time is due
                        int scheduledTimeMinutes = -1;
                        if (schedule is DailySchedule dailySchedule)
                        {
                            var times = dailySchedule.TimesOfDay.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            var checkMinutes = (int)now.TimeOfDay.TotalMinutes;
                            
                            foreach (var timeStr in times)
                            {
                                if (int.TryParse(timeStr, out int timeMinutes) && 
                                    Math.Abs(checkMinutes - timeMinutes) <= 15)
                                {
                                    scheduledTimeMinutes = timeMinutes;
                                    break;
                                }
                            }
                        }
                        
                        if (scheduledTimeMinutes >= 0)
                        {
                            dueMedications.Add((medication, scheduledTimeMinutes));
                        }
                    }
                }
            }

            // Send notifications for due medications (only if not already notified today)
            var todayKey = now.Date.ToString("yyyy-MM-dd");
            foreach (var (medication, scheduledTimeMinutes) in dueMedications)
            {
                string notificationKey = $"{medication.Id}_{todayKey}";
                
                // Check if we've already notified for this medication/time today
                if (_notifiedToday.TryGetValue(notificationKey, out var notifiedTimes))
                {
                    if (notifiedTimes.Contains(scheduledTimeMinutes))
                    {
                        continue; // Already notified for this time today
                    }
                }
                else
                {
                    _notifiedToday[notificationKey] = new HashSet<int>();
                }

                // Send notification and record it
                await SendMedicationReminderAsync(medication);
                _notifiedToday[notificationKey].Add(scheduledTimeMinutes);
            }
            
            // Clean up old notification records (from previous days)
            var keysToRemove = _notifiedToday.Keys
                .Where(key => !key.EndsWith(todayKey))
                .ToList();
            
            foreach (var key in keysToRemove)
            {
                _notifiedToday.Remove(key);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash the service
            System.Diagnostics.Debug.WriteLine($"Error checking for due medications: {ex.Message}");
        }
    }

    /// <summary>
    /// Sends a reminder notification for a due medication.
    /// </summary>
    private async Task SendMedicationReminderAsync(Medication medication)
    {
        try
        {
            var page = Application.Current?.Windows[0].Page;
            if (page == null)
            {
                return;
            }

            // Get the person's name if available
            var person = await _database.GetPersonByIdAsync(medication.PersonId);
            var personName = person != null ? $"{person.FirstName} {person.LastName}" : "Patient";

            // Build the reminder message
            var message = $"Time to take {medication.Name} ({medication.Dosage})";
            if (!string.IsNullOrWhiteSpace(medication.Instructions))
            {
                message += $"\n\nInstructions: {medication.Instructions}";
            }

            // Show alert on the main thread
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await page.DisplayAlert(
                    "Medication Reminder",
                    message,
                    "OK");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sending medication reminder: {ex.Message}");
        }
    }

    /// <summary>
    /// Schedules notifications for a medication with a daily schedule.
    /// This is called when a medication is saved with a daily schedule.
    /// The timer-based checking will handle notifications automatically.
    /// </summary>
    public Task ScheduleNotificationsForMedicationAsync(int medicationId)
    {
        // The timer-based checking will handle notifications automatically
        // This method exists for future extensibility (e.g., platform-specific local notifications)
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Stop();
    }
}


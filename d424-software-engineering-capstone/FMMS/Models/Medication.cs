using System;
using SQLite;

namespace FMMS.Models;

/// <summary>
/// Represents a medication tracked in the FMMS system.
/// Demonstrates inheritance from BaseEntity, encapsulation through private fields,
/// and polymorphism via method overriding.
/// </summary>
public class Medication : BaseEntity
{
    private string _name = string.Empty;
    private string _dosage = string.Empty;
    private string _instructions = string.Empty;
    private string _prescribingDoctor = string.Empty;
    private string _pharmacy = string.Empty;

    /// <summary>
    /// The ID of the person this medication belongs to.
    /// Foreign key to Person.Id
    /// </summary>
    [Indexed]
    public int PersonId { get; set; }

    /// <summary>
    /// Navigation property - the person taking this medication.
    /// Not persisted to database (marked with [Ignore])
    /// </summary>
    [Ignore]
    public Person? Person { get; set; }

    /// <summary>
    /// Formatted schedule times for display (e.g., "8:00 AM, 2:00 PM").
    /// Not persisted to database (marked with [Ignore])
    /// </summary>
    [Ignore]
    public string ScheduleTimesDisplay { get; set; } = string.Empty;

    public string Name
    {
        get => _name;
        set => _name = value?.Trim() ?? string.Empty;
    }

    public string Dosage
    {
        get => _dosage;
        set => _dosage = value?.Trim() ?? string.Empty;
    }

    public string Instructions
    {
        get => _instructions;
        set => _instructions = value?.Trim() ?? string.Empty;
    }

    public string PrescribingDoctor
    {
        get => _prescribingDoctor;
        set => _prescribingDoctor = value?.Trim() ?? string.Empty;
    }

    public string Pharmacy
    {
        get => _pharmacy;
        set => _pharmacy = value?.Trim() ?? string.Empty;
    }

    public DateTime? PrescriptionDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int RefillsAuthorized { get; set; }
    public int RefillsRemaining { get; set; }
    public int CurrentSupply { get; set; }
    public int LowSupplyThreshold { get; set; } = 10;
    /// <summary>
    /// Number of pills/doses to take per dose event.
    /// Defaults to 1 if not specified.
    /// </summary>
    public int PillsPerDose { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public int GetDaysOfSupplyRemaining(int dosesPerDay = 1)
    {
        if (dosesPerDay <= 0)
        {
            return 0;
        }
        return CurrentSupply / dosesPerDay;
    }

    public bool IsSupplyLow()
        => CurrentSupply <= LowSupplyThreshold && CurrentSupply > 0;

    public bool IsOutOfStock()
        => CurrentSupply <= 0;

    public bool NeedsRefill()
        => IsSupplyLow() && RefillsRemaining > 0;

    public bool IsExpired()
        => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;

    public DateTime? GetEstimatedRunOutDate(int dosesPerDay = 1)
    {
        if (dosesPerDay <= 0 || CurrentSupply <= 0)
        {
            return null;
        }
        int daysRemaining = GetDaysOfSupplyRemaining(dosesPerDay);
        return DateTime.UtcNow.AddDays(daysRemaining);
    }

    public void TakeDose(int dosesCount = 1)
    {
        if (dosesCount <= 0)
        {
            throw new ArgumentException("Doses count must be positive.", nameof(dosesCount));
        }
        CurrentSupply -= dosesCount;
        if (CurrentSupply < 0)
        {
            CurrentSupply = 0;
        }
        MarkAsUpdated();
    }

    public void RecordRefill(int pillsCount)
    {
        if (pillsCount <= 0)
        {
            throw new ArgumentException("Pills count must be positive.", nameof(pillsCount));
        }
        CurrentSupply += pillsCount;
        if (RefillsRemaining > 0)
        {
            RefillsRemaining--;
        }
        MarkAsUpdated();
    }

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(Dosage))
        {
            return Name;
        }
        return $"{Name} ({Dosage})";
    }

    public override bool Validate()
    {
        if (!base.Validate())
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(Name))
        {
            return false;
        }
        if (PersonId <= 0)
        {
            return false;
        }
        if (PrescriptionDate.HasValue && PrescriptionDate.Value > DateTime.UtcNow)
        {
            return false;
        }
        if (PrescriptionDate.HasValue && ExpirationDate.HasValue &&
            ExpirationDate.Value < PrescriptionDate.Value)
        {
            return false;
        }
        if (RefillsRemaining > RefillsAuthorized)
        {
            return false;
        }
        if (CurrentSupply < 0 || RefillsAuthorized < 0 || RefillsRemaining < 0)
        {
            return false;
        }
        if (LowSupplyThreshold < 0)
        {
            return false;
        }
        return true;
    }
}
using System;
using FMMS.Models;
using Xunit;

namespace FMMS.Tests;

/// <summary>
/// Unit tests for ScheduleRule hierarchy demonstrating polymorphism.
/// Tests abstract base class, concrete implementations, factory pattern, and polymorphic behavior.
/// </summary>
public class ScheduleRuleTests
{
    #region Inheritance Tests

    [Fact]
    public void DailySchedule_InheritsFromScheduleRule()
    {
        // Arrange & Act
        var schedule = new DailySchedule { MedicationId = 1, ScheduleType = "Daily" };

        // Assert - Demonstrates inheritance
        Assert.IsAssignableFrom<ScheduleRule>(schedule);
        Assert.IsAssignableFrom<BaseEntity>(schedule);
    }

    [Fact]
    public void IntervalSchedule_InheritsFromScheduleRule()
    {
        // Arrange & Act
        var schedule = new IntervalSchedule { MedicationId = 1, ScheduleType = "Interval" };

        // Assert - Demonstrates inheritance
        Assert.IsAssignableFrom<ScheduleRule>(schedule);
        Assert.IsAssignableFrom<BaseEntity>(schedule);
    }

    [Fact]
    public void WeeklySchedule_InheritsFromScheduleRule()
    {
        // Arrange & Act
        var schedule = new WeeklySchedule { MedicationId = 1, ScheduleType = "Weekly" };

        // Assert - Demonstrates inheritance
        Assert.IsAssignableFrom<ScheduleRule>(schedule);
        Assert.IsAssignableFrom<BaseEntity>(schedule);
    }

    [Fact]
    public void AsNeededSchedule_InheritsFromScheduleRule()
    {
        // Arrange & Act
        var schedule = new AsNeededSchedule { MedicationId = 1, ScheduleType = "AsNeeded" };

        // Assert - Demonstrates inheritance
        Assert.IsAssignableFrom<ScheduleRule>(schedule);
        Assert.IsAssignableFrom<BaseEntity>(schedule);
    }

    #endregion

    #region Factory Pattern Tests

    [Fact]
    public void CreateSchedule_Daily_ReturnsDailySchedule()
    {
        // Act
        var schedule = ScheduleRule.CreateSchedule("Daily", 1);

        // Assert - Factory creates correct type
        Assert.IsType<DailySchedule>(schedule);
        Assert.Equal("Daily", schedule.ScheduleType);
        Assert.Equal(1, schedule.MedicationId);
    }

    [Fact]
    public void CreateSchedule_Interval_ReturnsIntervalSchedule()
    {
        // Act
        var schedule = ScheduleRule.CreateSchedule("Interval", 2);

        // Assert - Factory creates correct type
        Assert.IsType<IntervalSchedule>(schedule);
        Assert.Equal("Interval", schedule.ScheduleType);
        Assert.Equal(2, schedule.MedicationId);
    }

    [Fact]
    public void CreateSchedule_Weekly_ReturnsWeeklySchedule()
    {
        // Act
        var schedule = ScheduleRule.CreateSchedule("Weekly", 3);

        // Assert - Factory creates correct type
        Assert.IsType<WeeklySchedule>(schedule);
        Assert.Equal("Weekly", schedule.ScheduleType);
        Assert.Equal(3, schedule.MedicationId);
    }

    [Fact]
    public void CreateSchedule_AsNeeded_ReturnsAsNeededSchedule()
    {
        // Act
        var schedule = ScheduleRule.CreateSchedule("AsNeeded", 4);

        // Assert - Factory creates correct type
        Assert.IsType<AsNeededSchedule>(schedule);
        Assert.Equal("AsNeeded", schedule.ScheduleType);
        Assert.Equal(4, schedule.MedicationId);
    }

    [Fact]
    public void CreateSchedule_InvalidType_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ScheduleRule.CreateSchedule("Invalid", 1));
    }

    #endregion

    #region Polymorphism Tests - CalculateNextDose

    [Fact]
    public void CalculateNextDose_DailySchedule_ReturnsNextDailyTime()
    {
        // Arrange - Daily at 8 AM (480 minutes) and 8 PM (1200 minutes)
        var schedule = new DailySchedule
        {
            MedicationId = 1,
            ScheduleType = "Daily",
            TimesOfDay = "480,1200",
            IsActive = true
        };
        var now = new DateTime(2025, 11, 23, 14, 0, 0); // 2:00 PM

        // Act - Polymorphic call
        var nextDose = schedule.CalculateNextDose(now);

        // Assert - Daily schedule returns 8 PM today (next time today)
        Assert.NotNull(nextDose);
        Assert.Equal(now.Date.AddHours(20), nextDose.Value); // 8 PM
    }

    [Fact]
    public void CalculateNextDose_IntervalSchedule_ReturnsNextIntervalTime()
    {
        // Arrange - Every 4 hours, starting at 8 AM
        var schedule = new IntervalSchedule
        {
            MedicationId = 1,
            ScheduleType = "Interval",
            IntervalAmount = 4,
            IntervalUnit = "Hours",
            StartTime = new DateTime(2025, 11, 23, 8, 0, 0),
            IsActive = true
        };
        var now = new DateTime(2025, 11, 23, 14, 0, 0); // 2:00 PM

        // Act - Polymorphic call
        var nextDose = schedule.CalculateNextDose(now);

        // Assert - Next dose at 4 PM (8 AM + 8 hours = 4 PM)
        Assert.NotNull(nextDose);
        Assert.Equal(new DateTime(2025, 11, 23, 16, 0, 0), nextDose.Value);
    }

    [Fact]
    public void CalculateNextDose_WeeklySchedule_ReturnsNextWeeklyTime()
    {
        // Arrange - Monday and Wednesday at 9 AM
        var schedule = new WeeklySchedule
        {
            MedicationId = 1,
            ScheduleType = "Weekly",
            DaysOfWeek = "Monday,Wednesday",
            TimeOfDay = 540, // 9:00 AM
            IsActive = true
        };
        var now = new DateTime(2025, 11, 23, 14, 0, 0); // Sunday 2:00 PM

        // Act - Polymorphic call
        var nextDose = schedule.CalculateNextDose(now);

        // Assert - Next dose Monday at 9 AM
        Assert.NotNull(nextDose);
        Assert.Equal(DayOfWeek.Monday, nextDose.Value.DayOfWeek);
        Assert.Equal(9, nextDose.Value.Hour);
    }

    [Fact]
    public void CalculateNextDose_AsNeededSchedule_ReturnsNull()
    {
        // Arrange
        var schedule = new AsNeededSchedule
        {
            MedicationId = 1,
            ScheduleType = "AsNeeded",
            IsActive = true
        };
        var now = DateTime.UtcNow;

        // Act - Polymorphic call
        var nextDose = schedule.CalculateNextDose(now);

        // Assert - As-needed has no scheduled next dose
        Assert.Null(nextDose);
    }

    [Fact]
    public void CalculateNextDose_PolymorphicCollection_EachTypeBehavesDifferently()
    {
        // Arrange - Create collection of different schedule types (polymorphism)
        var schedules = new List<ScheduleRule>
        {
            new DailySchedule { MedicationId = 1, ScheduleType = "Daily", TimesOfDay = "480", IsActive = true },
            new IntervalSchedule { MedicationId = 2, ScheduleType = "Interval", IntervalAmount = 6, IntervalUnit = "Hours", StartTime = DateTime.UtcNow.AddHours(-2), IsActive = true },
            new WeeklySchedule { MedicationId = 3, ScheduleType = "Weekly", DaysOfWeek = "Monday", TimeOfDay = 540, IsActive = true },
            new AsNeededSchedule { MedicationId = 4, ScheduleType = "AsNeeded", IsActive = true }
        };

        var now = DateTime.UtcNow;

        // Act - Call polymorphic method on each type
        var results = schedules.Select(s => s.CalculateNextDose(now)).ToList();

        // Assert - Each type behaves differently (polymorphism in action)
        Assert.NotNull(results[0]); // Daily schedule
        Assert.NotNull(results[1]); // Interval schedule
        Assert.NotNull(results[2]); // Weekly schedule
        Assert.Null(results[3]); // As-needed returns null

        // Each returns a different result based on its implementation
        Assert.NotEqual(results[0], results[1]);
    }

    #endregion

    #region Polymorphism Tests - IsDueAt

    [Fact]
    public void IsDueAt_DailySchedule_ChecksTimeOfDay()
    {
        // Arrange - Daily at 8 AM
        var schedule = new DailySchedule
        {
            MedicationId = 1,
            ScheduleType = "Daily",
            TimesOfDay = "480", // 8:00 AM
            IsActive = true
        };
        var checkTime = new DateTime(2025, 11, 23, 8, 5, 0); // 8:05 AM

        // Act - Polymorphic call
        var isDue = schedule.IsDueAt(checkTime);

        // Assert - Due within 15-minute window
        Assert.True(isDue);
    }

    [Fact]
    public void IsDueAt_IntervalSchedule_ChecksInterval()
    {
        // Arrange - Every 4 hours
        var schedule = new IntervalSchedule
        {
            MedicationId = 1,
            ScheduleType = "Interval",
            IntervalAmount = 4,
            IntervalUnit = "Hours",
            StartTime = new DateTime(2025, 11, 23, 8, 0, 0),
            IsActive = true
        };
        var checkTime = new DateTime(2025, 11, 23, 12, 5, 0); // 4 hours + 5 min after start

        // Act - Polymorphic call
        var isDue = schedule.IsDueAt(checkTime);

        // Assert - Due at interval time (within tolerance)
        Assert.True(isDue);
    }

    [Fact]
    public void IsDueAt_AsNeededSchedule_AlwaysDue()
    {
        // Arrange
        var schedule = new AsNeededSchedule
        {
            MedicationId = 1,
            ScheduleType = "AsNeeded",
            IsActive = true,
            MinimumHoursBetweenDoses = 0
        };
        var checkTime = DateTime.UtcNow;

        // Act - Polymorphic call
        var isDue = schedule.IsDueAt(checkTime);

        // Assert - As-needed is always due (if no minimum interval)
        Assert.True(isDue);
    }

    #endregion

    #region Polymorphism Tests - GetScheduleDescription

    [Fact]
    public void GetScheduleDescription_DailySchedule_ReturnsDailyDescription()
    {
        // Arrange
        var schedule = new DailySchedule
        {
            MedicationId = 1,
            ScheduleType = "Daily",
            TimesOfDay = "480,1200" // 8 AM and 8 PM
        };

        // Act - Polymorphic call
        var description = schedule.GetScheduleDescription();

        // Assert - Daily-specific description
        Assert.Contains("Daily", description);
        Assert.Contains("8:00 AM", description);
    }

    [Fact]
    public void GetScheduleDescription_IntervalSchedule_ReturnsIntervalDescription()
    {
        // Arrange
        var schedule = new IntervalSchedule
        {
            MedicationId = 1,
            ScheduleType = "Interval",
            IntervalAmount = 6,
            IntervalUnit = "Hours"
        };

        // Act - Polymorphic call
        var description = schedule.GetScheduleDescription();

        // Assert - Interval-specific description
        Assert.Contains("Every 6 hour(s)", description);
    }

    [Fact]
    public void GetScheduleDescription_WeeklySchedule_ReturnsWeeklyDescription()
    {
        // Arrange
        var schedule = new WeeklySchedule
        {
            MedicationId = 1,
            ScheduleType = "Weekly",
            DaysOfWeek = "Monday,Wednesday,Friday",
            TimeOfDay = 540 // 9 AM
        };

        // Act - Polymorphic call
        var description = schedule.GetScheduleDescription();

        // Assert - Weekly-specific description
        Assert.Contains("Weekly", description);
        Assert.Contains("Monday", description);
    }

    [Fact]
    public void GetScheduleDescription_AsNeededSchedule_ReturnsAsNeededDescription()
    {
        // Arrange
        var schedule = new AsNeededSchedule
        {
            MedicationId = 1,
            ScheduleType = "AsNeeded",
            MinimumHoursBetweenDoses = 4
        };

        // Act - Polymorphic call
        var description = schedule.GetScheduleDescription();

        // Assert - As-needed-specific description
        Assert.Contains("As needed", description);
        Assert.Contains("minimum", description);
    }

    [Fact]
    public void GetScheduleDescription_PolymorphicCollection_DifferentDescriptions()
    {
        // Arrange - Polymorphic collection
        var schedules = new List<ScheduleRule>
        {
            new DailySchedule { MedicationId = 1, ScheduleType = "Daily", TimesOfDay = "480" },
            new IntervalSchedule { MedicationId = 2, ScheduleType = "Interval", IntervalAmount = 4, IntervalUnit = "Hours" },
            new WeeklySchedule { MedicationId = 3, ScheduleType = "Weekly", DaysOfWeek = "Monday", TimeOfDay = 540 },
            new AsNeededSchedule { MedicationId = 4, ScheduleType = "AsNeeded" }
        };

        // Act - Call polymorphic method
        var descriptions = schedules.Select(s => s.GetScheduleDescription()).ToList();

        // Assert - Each type returns different description (polymorphism)
        Assert.Contains("Daily", descriptions[0]);
        Assert.Contains("Every", descriptions[1]);
        Assert.Contains("Weekly", descriptions[2]);
        Assert.Contains("As needed", descriptions[3]);

        // All descriptions are different
        Assert.All(descriptions, desc => Assert.NotEmpty(desc));
        Assert.Equal(4, descriptions.Distinct().Count());
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void ScheduleRule_Validate_RequiresMedicationId()
    {
        // Arrange
        var schedule = new DailySchedule { ScheduleType = "Daily" }; // No MedicationId

        // Act
        var isValid = schedule.Validate();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ScheduleRule_Validate_RequiresScheduleType()
    {
        // Arrange
        var schedule = new DailySchedule { MedicationId = 1 }; // No ScheduleType

        // Act
        var isValid = schedule.Validate();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ScheduleRule_Validate_ValidSchedule_ReturnsTrue()
    {
        // Arrange
        var schedule = new DailySchedule { MedicationId = 1, ScheduleType = "Daily" };

        // Act
        var isValid = schedule.Validate();

        // Assert
        Assert.True(isValid);
    }

    #endregion

    #region Encapsulation Tests

    [Fact]
    public void ScheduleRule_IsActive_CanBeSetAndGet()
    {
        // Arrange
        var schedule = new DailySchedule { MedicationId = 1, ScheduleType = "Daily" };

        // Act
        schedule.IsActive = false;

        // Assert
        Assert.False(schedule.IsActive);
        Assert.Null(schedule.CalculateNextDose(DateTime.UtcNow)); // Inactive schedules return null
    }

    [Fact]
    public void AsNeededSchedule_RecordDoseTaken_UpdatesLastDoseTime()
    {
        // Arrange
        var schedule = new AsNeededSchedule
        {
            MedicationId = 1,
            ScheduleType = "AsNeeded",
            MinimumHoursBetweenDoses = 4
        };
        var doseTime = DateTime.UtcNow;

        // Act
        schedule.RecordDoseTaken(doseTime);

        // Assert
        Assert.Equal(doseTime, schedule.LastDoseTime);
    }

    #endregion
}


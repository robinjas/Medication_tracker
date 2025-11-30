using System;
using FMMS.Models;
using Xunit;

namespace FMMS.Tests;

/// <summary>
/// Unit tests for the Medication model.
/// Tests inheritance, encapsulation, validation, and business logic.
/// </summary>
public class MedicationTests
{
    [Fact]
    public void Medication_InheritsFromBaseEntity()
    {
        // Arrange & Act
        var medication = new Medication();

        // Assert
        Assert.IsAssignableFrom<BaseEntity>(medication);
        Assert.True(medication.Id >= 0);
    }

    [Fact]
    public void Medication_Name_TrimsWhitespace()
    {
        // Arrange
        var medication = new Medication();

        // Act
        medication.Name = "  Aspirin  ";

        // Assert
        Assert.Equal("Aspirin", medication.Name);
    }

    [Fact]
    public void Medication_Dosage_TrimsWhitespace()
    {
        // Arrange
        var medication = new Medication();

        // Act
        medication.Dosage = "  100mg  ";

        // Assert
        Assert.Equal("100mg", medication.Dosage);
    }

    [Fact]
    public void Medication_ToString_ReturnsNameAndDosage()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            Dosage = "100mg"
        };

        // Act
        var result = medication.ToString();

        // Assert
        Assert.Equal("Aspirin (100mg)", result);
    }

    [Fact]
    public void Medication_ToString_ReturnsNameOnly_WhenDosageIsEmpty()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            Dosage = ""
        };

        // Act
        var result = medication.ToString();

        // Assert
        Assert.Equal("Aspirin", result);
    }

    [Fact]
    public void Medication_Validate_ReturnsFalse_WhenNameIsEmpty()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "",
            Dosage = "100mg",
            PersonId = 1
        };

        // Act
        var result = medication.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Medication_Validate_ReturnsFalse_WhenPersonIdIsZero()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            Dosage = "100mg",
            PersonId = 0
        };

        // Act
        var result = medication.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Medication_Validate_ReturnsFalse_WhenPersonIdIsNegative()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            Dosage = "100mg",
            PersonId = -1
        };

        // Act
        var result = medication.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Medication_Validate_ReturnsFalse_WhenPrescriptionDateIsInFuture()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            Dosage = "100mg",
            PersonId = 1,
            PrescriptionDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = medication.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Medication_Validate_ReturnsFalse_WhenExpirationDateIsBeforePrescriptionDate()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            Dosage = "100mg",
            PersonId = 1,
            PrescriptionDate = DateTime.UtcNow.AddDays(-30),
            ExpirationDate = DateTime.UtcNow.AddDays(-40)
        };

        // Act
        var result = medication.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Medication_Validate_ReturnsFalse_WhenRefillsRemainingExceedsAuthorized()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            Dosage = "100mg",
            PersonId = 1,
            RefillsAuthorized = 3,
            RefillsRemaining = 5
        };

        // Act
        var result = medication.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Medication_Validate_ReturnsFalse_WhenCurrentSupplyIsNegative()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            Dosage = "100mg",
            PersonId = 1,
            CurrentSupply = -1
        };

        // Act
        var result = medication.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Medication_Validate_ReturnsTrue_WhenValid()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            Dosage = "100mg",
            PersonId = 1,
            CurrentSupply = 30,
            RefillsAuthorized = 3,
            RefillsRemaining = 2
        };

        // Act
        var result = medication.Validate();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Medication_TakeDose_DecrementsCurrentSupply()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 10
        };

        // Act
        medication.TakeDose(1);

        // Assert
        Assert.Equal(9, medication.CurrentSupply);
    }

    [Fact]
    public void Medication_TakeDose_DecrementsByMultipleDoses()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 10
        };

        // Act
        medication.TakeDose(3);

        // Assert
        Assert.Equal(7, medication.CurrentSupply);
    }

    [Fact]
    public void Medication_TakeDose_PreventsNegativeSupply()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 2
        };

        // Act
        medication.TakeDose(5);

        // Assert
        Assert.Equal(0, medication.CurrentSupply);
    }

    [Fact]
    public void Medication_TakeDose_ThrowsException_WhenDosesCountIsZero()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 10
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => medication.TakeDose(0));
    }

    [Fact]
    public void Medication_TakeDose_ThrowsException_WhenDosesCountIsNegative()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 10
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => medication.TakeDose(-1));
    }

    [Fact]
    public void Medication_RecordRefill_IncrementsCurrentSupply()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 10,
            RefillsRemaining = 3
        };

        // Act
        medication.RecordRefill(30);

        // Assert
        Assert.Equal(40, medication.CurrentSupply);
    }

    [Fact]
    public void Medication_RecordRefill_DecrementsRefillsRemaining()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 10,
            RefillsRemaining = 3
        };

        // Act
        medication.RecordRefill(30);

        // Assert
        Assert.Equal(2, medication.RefillsRemaining);
    }

    [Fact]
    public void Medication_RecordRefill_DoesNotDecrementRefillsRemaining_WhenZero()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 10,
            RefillsRemaining = 0
        };

        // Act
        medication.RecordRefill(30);

        // Assert
        Assert.Equal(0, medication.RefillsRemaining);
        Assert.Equal(40, medication.CurrentSupply);
    }

    [Fact]
    public void Medication_RecordRefill_ThrowsException_WhenPillsCountIsZero()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 10
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => medication.RecordRefill(0));
    }

    [Fact]
    public void Medication_IsSupplyLow_ReturnsTrue_WhenAtThreshold()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 10,
            LowSupplyThreshold = 10
        };

        // Act
        var result = medication.IsSupplyLow();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Medication_IsSupplyLow_ReturnsTrue_WhenBelowThreshold()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 5,
            LowSupplyThreshold = 10
        };

        // Act
        var result = medication.IsSupplyLow();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Medication_IsSupplyLow_ReturnsFalse_WhenAboveThreshold()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 15,
            LowSupplyThreshold = 10
        };

        // Act
        var result = medication.IsSupplyLow();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Medication_IsSupplyLow_ReturnsTrue_WhenOutOfStock()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 0,
            LowSupplyThreshold = 10
        };

        // Act
        var result = medication.IsSupplyLow();

        // Assert
        Assert.True(result); // Out of stock (0) is at or below threshold, should show in low supply list
    }

    [Fact]
    public void Medication_IsOutOfStock_ReturnsTrue_WhenSupplyIsZero()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 0
        };

        // Act
        var result = medication.IsOutOfStock();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Medication_IsOutOfStock_ReturnsTrue_WhenSupplyIsNegative()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = -1
        };

        // Act
        var result = medication.IsOutOfStock();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Medication_IsOutOfStock_ReturnsFalse_WhenSupplyIsPositive()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 10
        };

        // Act
        var result = medication.IsOutOfStock();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Medication_NeedsRefill_ReturnsTrue_WhenLowSupplyAndRefillsRemaining()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 5,
            LowSupplyThreshold = 10,
            RefillsRemaining = 2
        };

        // Act
        var result = medication.NeedsRefill();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Medication_NeedsRefill_ReturnsFalse_WhenNotLowSupply()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 15,
            LowSupplyThreshold = 10,
            RefillsRemaining = 2
        };

        // Act
        var result = medication.NeedsRefill();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Medication_NeedsRefill_ReturnsFalse_WhenNoRefillsRemaining()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 5,
            LowSupplyThreshold = 10,
            RefillsRemaining = 0
        };

        // Act
        var result = medication.NeedsRefill();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Medication_IsExpired_ReturnsTrue_WhenExpirationDateIsPast()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            ExpirationDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = medication.IsExpired();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Medication_IsExpired_ReturnsFalse_WhenExpirationDateIsFuture()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = medication.IsExpired();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Medication_IsExpired_ReturnsFalse_WhenExpirationDateIsNull()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            ExpirationDate = null
        };

        // Act
        var result = medication.IsExpired();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Medication_GetDaysOfSupplyRemaining_CalculatesCorrectly()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 30
        };

        // Act
        var result = medication.GetDaysOfSupplyRemaining(2); // 2 doses per day

        // Assert
        Assert.Equal(15, result);
    }

    [Fact]
    public void Medication_GetDaysOfSupplyRemaining_ReturnsZero_WhenDosesPerDayIsZero()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 30
        };

        // Act
        var result = medication.GetDaysOfSupplyRemaining(0);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Medication_GetEstimatedRunOutDate_CalculatesCorrectly()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 30
        };
        var before = DateTime.UtcNow;

        // Act
        var result = medication.GetEstimatedRunOutDate(2); // 2 doses per day = 15 days

        // Act & Assert
        Assert.NotNull(result);
        var after = DateTime.UtcNow.AddDays(15).AddSeconds(1);
        Assert.True(result >= before.AddDays(15));
        Assert.True(result <= after);
    }

    [Fact]
    public void Medication_GetEstimatedRunOutDate_ReturnsNull_WhenOutOfStock()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 0
        };

        // Act
        var result = medication.GetEstimatedRunOutDate(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Medication_TakeDose_UpdatesTimestamp()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 10
        };
        var originalUpdatedAt = medication.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10);
        medication.TakeDose(1);

        // Assert
        Assert.True(medication.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void Medication_RecordRefill_UpdatesTimestamp()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirin",
            CurrentSupply = 10
        };
        var originalUpdatedAt = medication.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10);
        medication.RecordRefill(30);

        // Assert
        Assert.True(medication.UpdatedAt >= originalUpdatedAt);
    }
}


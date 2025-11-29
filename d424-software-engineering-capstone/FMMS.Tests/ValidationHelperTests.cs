using System;
using System.Linq;
using FMMS.Helpers;
using Xunit;

namespace FMMS.Tests;

/// <summary>
/// Unit tests for the ValidationHelper class.
/// Tests input validation, sanitization, and security features.
/// </summary>
public class ValidationHelperTests
{
    [Theory]
    [InlineData("John")]
    [InlineData("Mary-Jane")]
    [InlineData("O'Brien")]
    [InlineData("Jean Pierre")]
    public void IsValidName_ReturnsTrue_ForValidNames(string name)
    {
        // Act
        var result = ValidationHelper.IsValidName(name);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("A")] // Too short
    [InlineData("John123")] // Contains numbers
    [InlineData("John@Doe")] // Contains special character
    public void IsValidName_ReturnsFalse_ForInvalidNames(string? name)
    {
        // Act
        var result = ValidationHelper.IsValidName(name);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("Aspirin")]
    [InlineData("Ibuprofen 200mg")]
    [InlineData("Tylenol (Extra Strength)")]
    [InlineData("Vitamin D3")]
    public void IsValidMedicationName_ReturnsTrue_ForValidNames(string name)
    {
        // Act
        var result = ValidationHelper.IsValidMedicationName(name);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("A")] // Too short
    [InlineData("Med@Name")] // Contains @
    public void IsValidMedicationName_ReturnsFalse_ForInvalidNames(string? name)
    {
        // Act
        var result = ValidationHelper.IsValidMedicationName(name);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("100mg")]
    [InlineData("5.5ml")]
    [InlineData("10 tablets")]
    [InlineData("2.5mg")]
    public void IsValidDosage_ReturnsTrue_ForValidDosages(string dosage)
    {
        // Act
        var result = ValidationHelper.IsValidDosage(dosage);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("mg")] // No number
    [InlineData("100")] // No unit
    [InlineData("abc mg")] // Not a number
    public void IsValidDosage_ReturnsFalse_ForInvalidDosages(string? dosage)
    {
        // Act
        var result = ValidationHelper.IsValidDosage(dosage);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void IsValidQuantity_ReturnsTrue_ForValidQuantities(int quantity)
    {
        // Act
        var result = ValidationHelper.IsValidQuantity(quantity);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10001)]
    [InlineData(int.MaxValue)]
    public void IsValidQuantity_ReturnsFalse_ForInvalidQuantities(int quantity)
    {
        // Act
        var result = ValidationHelper.IsValidQuantity(quantity);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("555-123-4567")]
    [InlineData("(555) 123-4567")]
    [InlineData("5551234567")]
    [InlineData("1-555-123-4567")]
    public void IsValidPhoneNumber_ReturnsTrue_ForValidPhoneNumbers(string phoneNumber)
    {
        // Act
        var result = ValidationHelper.IsValidPhoneNumber(phoneNumber);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("123")] // Too short
    [InlineData("555-123")] // Too short
    [InlineData("abc-def-ghij")] // Contains letters
    public void IsValidPhoneNumber_ReturnsFalse_ForInvalidPhoneNumbers(string? phoneNumber)
    {
        // Act
        var result = ValidationHelper.IsValidPhoneNumber(phoneNumber);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FormatPhoneNumber_FormatsCorrectly()
    {
        // Arrange
        var phoneNumber = "5551234567";

        // Act
        var result = ValidationHelper.FormatPhoneNumber(phoneNumber);

        // Assert
        Assert.Equal("555-123-4567", result);
    }

    [Fact]
    public void FormatPhoneNumber_ReturnsOriginal_WhenCannotFormat()
    {
        // Arrange
        var phoneNumber = "123";

        // Act
        var result = ValidationHelper.FormatPhoneNumber(phoneNumber);

        // Assert
        Assert.Equal("123", result);
    }

    [Fact]
    public void IsValidPastDate_ReturnsTrue_ForPastDate()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = ValidationHelper.IsValidPastDate(date);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidPastDate_ReturnsFalse_ForFutureDate()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(1);

        // Act
        var result = ValidationHelper.IsValidPastDate(date);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidPastDate_ReturnsTrue_ForNullDate()
    {
        // Act
        var result = ValidationHelper.IsValidPastDate(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidFutureDate_ReturnsTrue_ForFutureDate()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(1);

        // Act
        var result = ValidationHelper.IsValidFutureDate(date);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidFutureDate_ReturnsFalse_ForPastDate()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = ValidationHelper.IsValidFutureDate(date);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidDateRange_ReturnsTrue_WhenEndDateIsAfterStartDate()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow.AddDays(30);

        // Act
        var result = ValidationHelper.IsValidDateRange(startDate, endDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidDateRange_ReturnsFalse_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = DateTime.UtcNow.AddDays(-30);

        // Act
        var result = ValidationHelper.IsValidDateRange(startDate, endDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidBirthDate_ReturnsTrue_ForValidBirthDate()
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddYears(-30);

        // Act
        var result = ValidationHelper.IsValidBirthDate(birthDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidBirthDate_ReturnsFalse_ForFutureDate()
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddDays(1);

        // Act
        var result = ValidationHelper.IsValidBirthDate(birthDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidBirthDate_ReturnsFalse_ForTooOld()
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddYears(-200);

        // Act
        var result = ValidationHelper.IsValidBirthDate(birthDate);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("#FF5733")]
    [InlineData("#000000")]
    [InlineData("#FFFFFF")]
    [InlineData("#123ABC")]
    public void IsValidHexColor_ReturnsTrue_ForValidColors(string color)
    {
        // Act
        var result = ValidationHelper.IsValidHexColor(color);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("#FF573")] // Too short
    [InlineData("FF5733")] // Missing #
    [InlineData("#GGGGGG")] // Invalid hex
    public void IsValidHexColor_ReturnsFalse_ForInvalidColors(string? color)
    {
        // Act
        var result = ValidationHelper.IsValidHexColor(color);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void SanitizeInput_RemovesDangerousSQLPatterns()
    {
        // Arrange
        var input = "John'; DROP TABLE People; --";

        // Act
        var result = ValidationHelper.SanitizeInput(input);

        // Assert
        Assert.DoesNotContain("'", result);
        Assert.DoesNotContain("--", result);
        Assert.DoesNotContain("DROP", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SanitizeInput_RemovesSQLKeywords()
    {
        // Arrange
        var input = "SELECT * FROM People WHERE Name = 'John'";

        // Act
        var result = ValidationHelper.SanitizeInput(input);

        // Assert
        Assert.DoesNotContain("SELECT", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("FROM", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SanitizeInput_ReturnsEmptyString_ForNull()
    {
        // Act
        var result = ValidationHelper.SanitizeInput(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ValidateMedicationData_ReturnsValid_WhenAllDataIsValid()
    {
        // Act
        var (isValid, errors) = ValidationHelper.ValidateMedicationData(
            "Aspirin",
            "100",
            "mg",
            30,
            15,
            1);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateMedicationData_ReturnsErrors_WhenNameIsInvalid()
    {
        // Act
        var (isValid, errors) = ValidationHelper.ValidateMedicationData(
            "",
            "100",
            "mg",
            30,
            15,
            1);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Medication name"));
    }

    [Fact]
    public void ValidateMedicationData_ReturnsErrors_WhenQuantityIsInvalid()
    {
        // Act
        var (isValid, errors) = ValidationHelper.ValidateMedicationData(
            "Aspirin",
            "100",
            "mg",
            -1,
            15,
            1);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Total quantity"));
    }

    [Fact]
    public void ValidatePersonData_ReturnsValid_WhenAllDataIsValid()
    {
        // Act
        var (isValid, errors) = ValidationHelper.ValidatePersonData(
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-30));

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidatePersonData_ReturnsErrors_WhenFirstNameIsInvalid()
    {
        // Act
        var (isValid, errors) = ValidationHelper.ValidatePersonData(
            "",
            "Doe",
            null);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("First name"));
    }

    [Theory]
    [InlineData("test", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsNotNullOrEmpty_ReturnsCorrectValue(string? value, bool expected)
    {
        // Act
        var result = ValidationHelper.IsNotNullOrEmpty(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test", true)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsNotNullOrWhiteSpace_ReturnsCorrectValue(string? value, bool expected)
    {
        // Act
        var result = ValidationHelper.IsNotNullOrWhiteSpace(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(5, 0, 10, true)]
    [InlineData(15, 0, 10, false)]
    [InlineData(-1, 0, 10, false)]
    public void IsInRange_ReturnsCorrectValue(int value, int min, int max, bool expected)
    {
        // Act
        var result = ValidationHelper.IsInRange(value, min, max);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsPositive_ReturnsCorrectValue(int value, bool expected)
    {
        // Act
        var result = ValidationHelper.IsPositive(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(-1, false)]
    public void IsNonNegative_ReturnsCorrectValue(int value, bool expected)
    {
        // Act
        var result = ValidationHelper.IsNonNegative(value);

        // Assert
        Assert.Equal(expected, result);
    }
}


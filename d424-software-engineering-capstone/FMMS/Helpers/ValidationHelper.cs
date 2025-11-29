using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace FMMS.Helpers;

/// <summary>
/// Centralized validation helper class for the FMMS application.
/// Provides validation methods for names, dosages, quantities, phone numbers, dates, and more.
/// Includes input sanitization used to normalize search terms and reduce obviously malicious patterns.
/// </summary>
public static class ValidationHelper
{
    // Regular expressions for validation
    private static readonly Regex NamePattern = new(@"^[a-zA-Z\s\-']{2,200}$", RegexOptions.Compiled);
    private static readonly Regex MedicationNamePattern = new(@"^[a-zA-Z0-9\s\-'()]{2,200}$", RegexOptions.Compiled);
    private static readonly Regex DosagePattern = new(@"^\d+(\.\d+)?\s*[a-zA-Z]+$", RegexOptions.Compiled);
    private static readonly Regex PhonePattern = new(@"^[\d\s\-()]+$", RegexOptions.Compiled);
    private static readonly Regex HexColorPattern = new(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    /// <summary>
    /// Validates a person's name (first or last name).
    /// Allows letters, spaces, hyphens, and apostrophes.
    /// </summary>
    public static bool IsValidName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return NamePattern.IsMatch(name.Trim()) && name.Trim().Length >= 2 && name.Trim().Length <= 200;
    }

    /// <summary>
    /// Validates a medication name.
    /// Allows letters, numbers, spaces, hyphens, apostrophes, and parentheses.
    /// </summary>
    public static bool IsValidMedicationName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return MedicationNamePattern.IsMatch(name.Trim()) && name.Trim().Length >= 2 && name.Trim().Length <= 200;
    }

    /// <summary>
    /// Validates a dosage string (e.g., "100mg", "5.5ml").
    /// Must contain a number (with optional decimal) followed by a unit.
    /// </summary>
    public static bool IsValidDosage(string? dosage)
    {
        if (string.IsNullOrWhiteSpace(dosage))
        {
            return false;
        }

        return DosagePattern.IsMatch(dosage.Trim());
    }

    /// <summary>
    /// Validates a quantity value (0 to 10,000).
    /// </summary>
    public static bool IsValidQuantity(int quantity)
    {
        return quantity >= 0 && quantity <= 10000;
    }

    /// <summary>
    /// Validates a phone number format.
    /// Allows digits, spaces, hyphens, and parentheses.
    /// </summary>
    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        return PhonePattern.IsMatch(phoneNumber) && cleaned.Length >= 10 && cleaned.Length <= 15;
    }

    /// <summary>
    /// Formats a phone number to a standard format (XXX-XXX-XXXX).
    /// </summary>
    public static string FormatPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return string.Empty;
        }

        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
        
        if (cleaned.Length == 10)
        {
            return $"{cleaned.Substring(0, 3)}-{cleaned.Substring(3, 3)}-{cleaned.Substring(6, 4)}";
        }

        return phoneNumber; // Return original if can't format
    }

    /// <summary>
    /// Validates a date is not in the future (for birth dates, prescription dates).
    /// </summary>
    public static bool IsValidPastDate(DateTime? date)
    {
        if (!date.HasValue)
        {
            return true; // Null dates are allowed
        }

        return date.Value <= DateTime.UtcNow;
    }

    /// <summary>
    /// Validates a date is not in the past (for future dates).
    /// </summary>
    public static bool IsValidFutureDate(DateTime? date)
    {
        if (!date.HasValue)
        {
            return true; // Null dates are allowed
        }

        return date.Value >= DateTime.UtcNow;
    }

    /// <summary>
    /// Validates that an expiration date is after a prescription date.
    /// </summary>
    public static bool IsValidDateRange(DateTime? startDate, DateTime? endDate)
    {
        if (!startDate.HasValue || !endDate.HasValue)
        {
            return true; // Null dates are allowed
        }

        return endDate.Value >= startDate.Value;
    }

    /// <summary>
    /// Validates a birth date is reasonable (not too far in the past, not in the future).
    /// </summary>
    public static bool IsValidBirthDate(DateTime? birthDate)
    {
        if (!birthDate.HasValue)
        {
            return true; // Null dates are allowed
        }

        var now = DateTime.UtcNow;
        var minDate = now.AddYears(-150); // Reasonable maximum age
        var maxDate = now; // Can't be in the future

        return birthDate.Value >= minDate && birthDate.Value <= maxDate;
    }

    /// <summary>
    /// Validates a hex color code (e.g., "#FF5733").
    /// </summary>
    public static bool IsValidHexColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return false;
        }

        return HexColorPattern.IsMatch(color.Trim());
    }

    /// <summary>
    /// Sanitizes input used for search to remove obvious SQL-like patterns
    /// and control tokens while preserving normal punctuation such as
    /// apostrophes, hyphens, and parentheses used in names and medication titles.
    /// </summary>
    public static string SanitizeInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var sanitized = input.Trim();

        // Remove common SQL comment/control tokens (keep normal punctuation like apostrophes)
        sanitized = sanitized.Replace("--", string.Empty)
                             .Replace("/*", string.Empty)
                             .Replace("*/", string.Empty)
                             .Replace(";", string.Empty);

        // Remove common stored-proc prefixes (rare in normal search text)
        sanitized = Regex.Replace(sanitized, @"\b(xp_|sp_)\w*\b", string.Empty, RegexOptions.IgnoreCase);

        // Remove SQL keywords only when they appear as whole words
        sanitized = Regex.Replace(
            sanitized,
            @"\b(select|insert|update|delete|drop|create|alter|from|where|execute|exec)\b",
            string.Empty,
            RegexOptions.IgnoreCase);

        // Collapse extra whitespace created by removals
        sanitized = Regex.Replace(sanitized, @"\s{2,}", " ").Trim();

        return sanitized;
    }

    /// <summary>
    /// Validates medication data with comprehensive checks.
    /// Returns validation result with list of errors.
    /// </summary>
    public static (bool IsValid, List<string> Errors) ValidateMedicationData(
        string? name,
        string? dosageAmount,
        string? dosageUnit,
        int totalQuantity,
        int remainingQuantity,
        int? personId = null)
    {
        var errors = new List<string>();

        if (!IsValidMedicationName(name))
        {
            errors.Add("Medication name must be 2-200 characters and contain only letters, numbers, spaces, hyphens, apostrophes, and parentheses.");
        }

        if (!string.IsNullOrWhiteSpace(dosageAmount))
        {
            var fullDosage = $"{dosageAmount} {dosageUnit}".Trim();
            if (!IsValidDosage(fullDosage))
            {
                errors.Add("Dosage must be in format: number (with optional decimal) followed by unit (e.g., '100mg', '5.5ml').");
            }
        }

        if (!IsValidQuantity(totalQuantity))
        {
            errors.Add($"Total quantity must be between 0 and 10,000. Current value: {totalQuantity}");
        }

        if (!IsValidQuantity(remainingQuantity))
        {
            errors.Add($"Remaining quantity must be between 0 and 10,000. Current value: {remainingQuantity}");
        }

        if (remainingQuantity > totalQuantity)
        {
            errors.Add($"Remaining quantity ({remainingQuantity}) cannot exceed total quantity ({totalQuantity}).");
        }

        if (personId.HasValue && personId.Value <= 0)
        {
            errors.Add("Person ID must be a positive number.");
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Validates person data with comprehensive checks.
    /// Returns validation result with list of errors.
    /// </summary>
    public static (bool IsValid, List<string> Errors) ValidatePersonData(
        string? firstName,
        string? lastName,
        DateTime? dateOfBirth = null)
    {
        var errors = new List<string>();

        if (!IsValidName(firstName))
        {
            errors.Add("First name must be 2-200 characters and contain only letters, spaces, hyphens, and apostrophes.");
        }

        if (!IsValidName(lastName))
        {
            errors.Add("Last name must be 2-200 characters and contain only letters, spaces, hyphens, and apostrophes.");
        }

        if (dateOfBirth.HasValue && !IsValidBirthDate(dateOfBirth))
        {
            errors.Add("Birth date must be in the past and not more than 150 years ago.");
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Validates that a string is not null or empty.
    /// </summary>
    public static bool IsNotNullOrEmpty(string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Validates that a string is not null, empty, or whitespace.
    /// </summary>
    public static bool IsNotNullOrWhiteSpace(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Validates that a number is within a specified range.
    /// </summary>
    public static bool IsInRange(int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Validates that a number is positive (greater than zero).
    /// </summary>
    public static bool IsPositive(int value)
    {
        return value > 0;
    }

    /// <summary>
    /// Validates that a number is non-negative (zero or greater).
    /// </summary>
    public static bool IsNonNegative(int value)
    {
        return value >= 0;
    }
}


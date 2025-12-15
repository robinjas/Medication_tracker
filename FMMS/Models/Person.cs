using System;

namespace FMMS.Models;

/// <summary>
/// Represents a person/family member in the medication management system.
/// Demonstrates inheritance from BaseEntity, encapsulation through private fields,
/// and polymorphism via method overriding.
/// </summary>
public class Person : BaseEntity
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;

    /// <summary>
    /// Gets or sets the person's first name. 
    /// Input is trimmed and nulls become an empty string.
    /// </summary>
    public string FirstName
    {
        get => _firstName;
        set => _firstName = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the person's last name.
    /// Input is trimmed and nulls become an empty string.
    /// </summary>
    public string LastName
    {
        get => _lastName;
        set => _lastName = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the person's date of birth.
    /// Nullable to allow unknown values.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Returns the full name (FirstName + LastName) for display and debugging.
    /// </summary>
    public override string ToString() => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Validates the person entity.
    /// </summary>
    public override bool Validate()
    {
        if (!base.Validate())
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(FirstName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            return false;
        }

        if (DateOfBirth.HasValue && DateOfBirth.Value > DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }
}

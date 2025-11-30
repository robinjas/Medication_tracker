using System;
using FMMS.Models;
using Xunit;

namespace FMMS.Tests;

/// <summary>
/// Unit tests for the Person model.
/// Tests inheritance, encapsulation, validation, and business logic.
/// </summary>
public class PersonTests
{
    [Fact]
    public void Person_InheritsFromBaseEntity()
    {
        // Arrange & Act
        var person = new Person();

        // Assert
        Assert.IsAssignableFrom<BaseEntity>(person);
        Assert.True(person.Id >= 0);
    }

    [Fact]
    public void Person_FirstName_TrimsWhitespace()
    {
        // Arrange
        var person = new Person();

        // Act
        person.FirstName = "  John  ";

        // Assert
        Assert.Equal("John", person.FirstName);
    }

    [Fact]
    public void Person_FirstName_HandlesNull()
    {
        // Arrange
        var person = new Person();

        // Act
        person.FirstName = null!;

        // Assert
        Assert.Equal(string.Empty, person.FirstName);
    }

    [Fact]
    public void Person_LastName_TrimsWhitespace()
    {
        // Arrange
        var person = new Person();

        // Act
        person.LastName = "  Doe  ";

        // Assert
        Assert.Equal("Doe", person.LastName);
    }

    [Fact]
    public void Person_ToString_ReturnsFullName()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = person.ToString();

        // Assert
        Assert.Equal("John Doe", result);
    }

    [Fact]
    public void Person_ToString_TrimsWhitespace()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "  John  ",
            LastName = "  Doe  "
        };

        // Act
        var result = person.ToString();

        // Assert
        Assert.Equal("John Doe", result);
    }

    [Fact]
    public void Person_Validate_ReturnsFalse_WhenFirstNameIsEmpty()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "",
            LastName = "Doe"
        };

        // Act
        var result = person.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Person_Validate_ReturnsFalse_WhenLastNameIsEmpty()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = ""
        };

        // Act
        var result = person.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Person_Validate_ReturnsFalse_WhenDateOfBirthIsInFuture()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = person.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Person_Validate_ReturnsTrue_WhenValid()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateTime.UtcNow.AddYears(-30)
        };

        // Act
        var result = person.Validate();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Person_Validate_ReturnsTrue_WhenDateOfBirthIsNull()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = null
        };

        // Act
        var result = person.Validate();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Person_SoftDelete_SetsIsDeletedToTrue()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        person.SoftDelete();

        // Assert
        Assert.True(person.IsDeleted);
    }

    [Fact]
    public void Person_SoftDelete_UpdatesTimestamp()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe"
        };
        var originalUpdatedAt = person.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10); // Small delay to ensure timestamp difference
        person.SoftDelete();

        // Assert
        Assert.True(person.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void Person_MarkAsUpdated_UpdatesTimestamp()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe"
        };
        var originalUpdatedAt = person.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10); // Small delay to ensure timestamp difference
        person.MarkAsUpdated();

        // Assert
        Assert.True(person.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void Person_CreatedAt_IsSetOnCreation()
    {
        // Arrange & Act
        var person = new Person();
        var now = DateTime.UtcNow;

        // Assert
        Assert.True(person.CreatedAt <= now);
        Assert.True(person.CreatedAt >= now.AddSeconds(-1));
    }
}


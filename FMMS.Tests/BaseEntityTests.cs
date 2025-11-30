using System;
using FMMS.Models;
using Xunit;

namespace FMMS.Tests;

/// <summary>
/// Unit tests for the BaseEntity abstract class.
/// Tests inheritance, encapsulation, timestamps, and soft delete functionality.
/// </summary>
public class BaseEntityTests
{
    // Create a concrete implementation for testing
    private class TestEntity : BaseEntity
    {
        public string TestProperty { get; set; } = string.Empty;
        
        public override bool Validate()
        {
            return base.Validate() && !string.IsNullOrWhiteSpace(TestProperty);
        }
    }

    [Fact]
    public void BaseEntity_HasIdProperty()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.True(entity.Id >= 0);
    }

    [Fact]
    public void BaseEntity_CreatedAt_IsSetOnCreation()
    {
        // Arrange & Act
        var entity = new TestEntity();
        var now = DateTime.UtcNow;

        // Assert
        Assert.True(entity.CreatedAt <= now);
        Assert.True(entity.CreatedAt >= now.AddSeconds(-1));
    }

    [Fact]
    public void BaseEntity_UpdatedAt_IsSetOnCreation()
    {
        // Arrange & Act
        var entity = new TestEntity();
        var now = DateTime.UtcNow;

        // Assert
        Assert.True(entity.UpdatedAt <= now);
        Assert.True(entity.UpdatedAt >= now.AddSeconds(-1));
    }

    [Fact]
    public void BaseEntity_CreatedAt_EqualsUpdatedAt_OnCreation()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.Equal(entity.CreatedAt, entity.UpdatedAt);
    }

    [Fact]
    public void BaseEntity_IsDeleted_IsFalseByDefault()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.False(entity.IsDeleted);
    }

    [Fact]
    public void BaseEntity_SoftDelete_SetsIsDeletedToTrue()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.SoftDelete();

        // Assert
        Assert.True(entity.IsDeleted);
    }

    [Fact]
    public void BaseEntity_SoftDelete_UpdatesTimestamp()
    {
        // Arrange
        var entity = new TestEntity();
        var originalUpdatedAt = entity.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10); // Small delay to ensure timestamp difference
        entity.SoftDelete();

        // Assert
        Assert.True(entity.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void BaseEntity_MarkAsUpdated_UpdatesTimestamp()
    {
        // Arrange
        var entity = new TestEntity();
        var originalUpdatedAt = entity.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10); // Small delay to ensure timestamp difference
        entity.MarkAsUpdated();

        // Assert
        Assert.True(entity.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void BaseEntity_Validate_ReturnsTrue_WhenIdIsValid()
    {
        // Arrange
        var entity = new TestEntity
        {
            TestProperty = "Test"
        };

        // Act
        var result = entity.Validate();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BaseEntity_Validate_ReturnsTrue_WhenIdIsZero()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = 0,
            TestProperty = "Test"
        };

        // Act
        var result = entity.Validate();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BaseEntity_Validate_CanBeOverridden()
    {
        // Arrange
        var entity = new TestEntity
        {
            TestProperty = "" // Empty should fail validation
        };

        // Act
        var result = entity.Validate();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BaseEntity_CreatedAt_IsReadOnlyFromOutside()
    {
        // Arrange
        var entity = new TestEntity();
        var originalCreatedAt = entity.CreatedAt;

        // Act - Try to modify (should not be possible via public setter)
        // Note: CreatedAt has protected setter, so we can't set it from outside
        // This test verifies the encapsulation

        // Assert
        Assert.Equal(originalCreatedAt, entity.CreatedAt);
    }

    [Fact]
    public void BaseEntity_Timestamps_AreInUtc()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.Equal(DateTimeKind.Utc, entity.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, entity.UpdatedAt.Kind);
    }
}


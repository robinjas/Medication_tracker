using System;
using SQLite;

namespace FMMS.Models;

/// <summary>
/// Base type for all persisted entities in the FMMS app.
/// Provides identity, timestamps, and soft-delete behavior.
/// Demonstrates inheritance (as a common parent type) and encapsulation
/// through protected setters and private backing fields.
/// </summary>
public abstract class BaseEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    private DateTime _createdAt;
    private DateTime _updatedAt;

    /// <summary>
    /// When the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt
    {
        get => _createdAt;
        protected set => _createdAt = value;
    }

    /// <summary>
    /// When the entity was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAt
    {
        get => _updatedAt;
        protected set => _updatedAt = value;
    }

    /// <summary>
    /// Indicates a soft-delete instead of removing the record from the database.
    /// Public setter required for SQLite-net deserialization.
    /// </summary>
    public bool IsDeleted { get; set; }  // <- public set

    protected BaseEntity()
    {
        var now = DateTime.UtcNow;
        _createdAt = now;
        _updatedAt = now;
    }

    /// <summary>
    /// Base validation. Derived types can extend this.
    /// </summary>
    public virtual bool Validate() => Id >= 0;

    /// <summary>
    /// Updates the UpdatedAt timestamp to the current UTC time.
    /// </summary>
    public void MarkAsUpdated()
    {
        _updatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the entity as soft-deleted and updates the timestamp.
    /// </summary>
    public virtual void SoftDelete()
    {
        IsDeleted = true;
        MarkAsUpdated();
    }
}

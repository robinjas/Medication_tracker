using System;

namespace FMMS.Models;

/// <summary>
/// Represents a unified search result from searching across multiple entity types.
/// Used by SearchService to return results from People, Medications, and other entities.
/// </summary>
public class SearchResult
{
    /// <summary>
    /// The type of entity (e.g., "Person", "Medication").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The primary title/name to display for this result.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Additional subtitle or description.
    /// </summary>
    public string Subtitle { get; set; } = string.Empty;

    /// <summary>
    /// Additional details about the result.
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the entity (for navigation/selection).
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Optional: Person ID if this is a medication result.
    /// </summary>
    public int? PersonId { get; set; }

    /// <summary>
    /// The actual entity object (Person, Medication, etc.).
    /// </summary>
    public object? Entity { get; set; }

    /// <summary>
    /// When this entity was created or last updated.
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// Returns a display-friendly string representation.
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(Subtitle))
        {
            return $"{Title} - {Subtitle}";
        }
        return Title;
    }
}


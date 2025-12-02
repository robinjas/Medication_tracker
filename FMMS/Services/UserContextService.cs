using System;

namespace FMMS.Services;

/// <summary>
/// Provides the current user context for multi-user support.
/// Initially returns null for single-user mode, but can be extended
/// to return the current logged-in user ID when authentication is added.
/// 
/// This service allows all database operations to be filtered by user
/// without changing the core business logic.
/// </summary>
public interface IUserContextService
{
    /// <summary>
    /// Gets the current user ID, or null if no user is logged in (single-user mode).
    /// </summary>
    int? CurrentUserId { get; }
    
    /// <summary>
    /// Checks if a user is currently logged in.
    /// </summary>
    bool IsUserLoggedIn { get; }
}

/// <summary>
/// Default implementation that supports both single-user and multi-user modes.
/// </summary>
public class UserContextService : IUserContextService
{
    private int? _currentUserId;

    public int? CurrentUserId => _currentUserId;
    public bool IsUserLoggedIn => _currentUserId.HasValue;

    /// <summary>
    /// Sets the current user (called after login).
    /// </summary>
    public void SetCurrentUser(int userId)
    {
        _currentUserId = userId;
    }

    /// <summary>
    /// Clears the current user (called after logout).
    /// </summary>
    public void ClearCurrentUser()
    {
        _currentUserId = null;
    }
}


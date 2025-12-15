using Microsoft.Maui.Controls;
using System.Linq;
using System.Threading.Tasks;

namespace FMMS.Helpers;

/// <summary>
/// Helper class for safely displaying alerts and dialogs in the application.
/// Handles null-checking to prevent crashes when Application.Current or Page is null.
/// </summary>
public static class DialogHelper
{
    /// <summary>
    /// Safely displays an alert dialog with null-checking.
    /// Returns false if unable to display (e.g., no page available).
    /// </summary>
    public static async Task<bool> ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        var page = GetCurrentPage();
        return await ShowAlertAsync(page, title, message, cancel);
    }

    /// <summary>
    /// Safely displays an alert dialog using the provided page.
    /// Returns false if page is null or if unable to display.
    /// </summary>
    public static async Task<bool> ShowAlertAsync(Page? page, string title, string message, string cancel = "OK")
    {
        try
        {
            if (page == null)
            {
                // Log or handle case where no page is available
                System.Diagnostics.Debug.WriteLine($"Unable to show alert: {title} - {message}");
                return false;
            }

            await page.DisplayAlert(title, message, cancel);
            return true;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing alert: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Safely displays a confirmation dialog (Yes/No) with null-checking.
    /// Returns null if unable to display the dialog.
    /// </summary>
    public static async Task<bool?> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        var page = GetCurrentPage();
        return await ShowConfirmationAsync(page, title, message, accept, cancel);
    }

    /// <summary>
    /// Safely displays a confirmation dialog (Yes/No) using the provided page.
    /// Returns null if page is null or if unable to display the dialog.
    /// </summary>
    public static async Task<bool?> ShowConfirmationAsync(Page? page, string title, string message, string accept = "Yes", string cancel = "No")
    {
        try
        {
            if (page == null)
            {
                System.Diagnostics.Debug.WriteLine($"Unable to show confirmation: {title} - {message}");
                return null;
            }

            var result = await page.DisplayAlert(title, message, accept, cancel);
            return result;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing confirmation: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets the current page from Application.Current.
    /// Returns null if no page is available.
    /// </summary>
    private static Page? GetCurrentPage()
    {
        return Application.Current?.Windows?.FirstOrDefault()?.Page;
    }
}

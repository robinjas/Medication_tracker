using System;
using System.Linq;
using System.Threading.Tasks;
using FMMS.Helpers;
using FMMS.Models;
using Microsoft.Maui.Controls;

namespace FMMS.Services;

public class MedicationActionService
{
    private readonly DatabaseService _database;

    public MedicationActionService(DatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async Task<bool> RefillAsync(Medication medication, Func<Task>? onRefresh = null)
    {
        if (medication == null)
        {
            return false;
        }

        // If there are no refills remaining, block the refill and instruct the user
        if (medication.RefillsRemaining <= 0)
        {
            await DialogHelper.ShowAlertAsync(
                "No Refills Remaining",
                $"{medication.Name} has no refills remaining on this prescription.\n\n" +
                "Please contact the prescribing doctor or pharmacy to request additional refills " +
                "before adding more supply.");
            return false;
        }

        var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
        if (page == null)
        {
            await DialogHelper.ShowAlertAsync("Error", "Unable to access the current page to record a refill.");
            return false;
        }

        // Prompt for refill amount
        var result = await page.DisplayPromptAsync(
            "Refill Medication",
            $"How many doses to add to {medication.Name}?",
            accept: "OK",
            cancel: "Cancel",
            initialValue: "30",
            maxLength: -1,
            keyboard: Keyboard.Numeric);

        if (result == null || !int.TryParse(result, out var amount) || amount <= 0)
        {
            // User cancelled or entered invalid amount
            return false;
        }

        try
        {
            medication.RecordRefill(amount);
            await _database.SaveMedicationAsync(medication);

            if (onRefresh != null)
            {
                await onRefresh();
            }

            await DialogHelper.ShowAlertAsync(
                "Success",
                $"Added {amount} doses to {medication.Name}. New supply: {medication.CurrentSupply}");

            return true;
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowAlertAsync("Error", $"Failed to record refill: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> TakeDoseAsync(Medication medication, Func<Task>? onRefresh = null)
    {
        if (medication == null)
        {
            return false;
        }
        // Block if out of stock, but don't show extra confirmation popups otherwise
        if (medication.IsOutOfStock())
        {
            await DialogHelper.ShowAlertAsync(
                "Out of Stock",
                $"{medication.Name} is out of stock. Please refill before taking a dose.");
            return false;
        }

        var dosesCount = medication.PillsPerDose > 0 ? medication.PillsPerDose : 1;
        var doseText = dosesCount == 1 ? "dose" : "doses";

        try
        {
            medication.TakeDose(dosesCount);
            await _database.SaveMedicationAsync(medication);

            var updatedMedication = await _database.GetMedicationByIdAsync(medication.Id);
            if (updatedMedication == null)
            {
                await DialogHelper.ShowAlertAsync("Error", "Failed to reload medication after taking dose.");
                return false;
            }

            if (onRefresh != null)
            {
                await onRefresh();
            }

            if (updatedMedication.IsSupplyLow())
            {
                var shouldRefill = await DialogHelper.ShowConfirmationAsync(
                    "Low Supply",
                    $"{updatedMedication.Name} is running low. Only {updatedMedication.CurrentSupply} doses remaining.\n\nTap Refill to add more doses so you don't run out.",
                    "Refill",
                    "Remind Me Later");

                if (shouldRefill == true)
                {
                    // Use the same refresh callback so the caller can update its UI
                    await RefillAsync(updatedMedication, onRefresh);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowAlertAsync("Error", $"Failed to record dose: {ex.Message}");
            return false;
        }
    }
}
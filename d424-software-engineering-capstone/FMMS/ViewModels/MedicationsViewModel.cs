using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FMMS.Models;
using FMMS.Services;
using Microsoft.Maui.Controls;

namespace FMMS.ViewModels;

/// <summary>
/// ViewModel for managing medications in the FMMS application.
/// Demonstrates MVVM pattern with INotifyPropertyChanged, Commands, and ObservableCollections.
/// </summary>
public class MedicationsViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _database;
    private readonly MedicationActionService _medicationActionService;
    private readonly NotificationService? _notificationService;

    public ObservableCollection<Medication> Medications { get; } = new();
    public ObservableCollection<Person> People { get; } = new();
    public ObservableCollection<ScheduleTimeViewModel> ScheduleTimes { get; } = new();

    private Medication? _selectedMedication;
    public Medication? SelectedMedication
    {
        get => _selectedMedication;
        set
        {
            if (SetProperty(ref _selectedMedication, value))
            {
                if (value != null)
                {
                    // Load medication data into form fields
                    Name = value.Name;
                    Dosage = value.Dosage;
                    Instructions = value.Instructions;
                    PrescribingDoctor = value.PrescribingDoctor;
                    Pharmacy = value.Pharmacy;
                    PrescriptionDate = value.PrescriptionDate;
                    ExpirationDate = value.ExpirationDate;
                    RefillsAuthorized = value.RefillsAuthorized;
                    RefillsRemaining = value.RefillsRemaining;
                    CurrentSupply = value.CurrentSupply;
                    LowSupplyThreshold = value.LowSupplyThreshold;
                    PillsPerDose = value.PillsPerDose;
                    IsActive = value.IsActive;
                    Notes = value.Notes;

                    // Set the selected person
                    SelectedPerson = People.FirstOrDefault(p => p.Id == value.PersonId);
                    
                    // Load schedule times
                    _ = LoadScheduleTimesAsync(value.Id);
                }
                else
                {
                    // Clear schedule times when no medication is selected
                    ScheduleTimes.Clear();
                }

                OnPropertyChanged(nameof(SaveButtonText));
                ((Command)SaveMedicationCommand).ChangeCanExecute();
            }
        }
    }

    private Person? _selectedPerson;
    public Person? SelectedPerson
    {
        get => _selectedPerson;
        set
        {
            if (SetProperty(ref _selectedPerson, value))
            {
                ((Command)SaveMedicationCommand).ChangeCanExecute();
                if (value != null && !IsBusy)
                {
                    _ = LoadMedicationsForPersonAsync(value.Id);
                }
            }
        }
    }

    public string SaveButtonText => SelectedMedication == null ? "Add" : "Update";

    // Form fields
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                ((Command)SaveMedicationCommand).ChangeCanExecute();
            }
        }
    }

    private string _dosage = string.Empty;
    public string Dosage
    {
        get => _dosage;
        set
        {
            if (SetProperty(ref _dosage, value))
            {
                ((Command)SaveMedicationCommand).ChangeCanExecute();
            }
        }
    }

    private string _instructions = string.Empty;
    public string Instructions
    {
        get => _instructions;
        set => SetProperty(ref _instructions, value);
    }

    private string _prescribingDoctor = string.Empty;
    public string PrescribingDoctor
    {
        get => _prescribingDoctor;
        set => SetProperty(ref _prescribingDoctor, value);
    }

    private string _pharmacy = string.Empty;
    public string Pharmacy
    {
        get => _pharmacy;
        set => SetProperty(ref _pharmacy, value);
    }

    private DateTime? _prescriptionDate;
    public DateTime? PrescriptionDate
    {
        get => _prescriptionDate;
        set => SetProperty(ref _prescriptionDate, value);
    }

    private DateTime? _expirationDate;
    public DateTime? ExpirationDate
    {
        get => _expirationDate;
        set => SetProperty(ref _expirationDate, value);
    }

    private int _refillsAuthorized;
    public int RefillsAuthorized
    {
        get => _refillsAuthorized;
        set => SetProperty(ref _refillsAuthorized, value);
    }

    private int _refillsRemaining;
    public int RefillsRemaining
    {
        get => _refillsRemaining;
        set => SetProperty(ref _refillsRemaining, value);
    }

    private int _currentSupply;
    public int CurrentSupply
    {
        get => _currentSupply;
        set => SetProperty(ref _currentSupply, value);
    }

    private int _lowSupplyThreshold = 10;
    public int LowSupplyThreshold
    {
        get => _lowSupplyThreshold;
        set => SetProperty(ref _lowSupplyThreshold, value);
    }

    private int _pillsPerDose = 1;
    public int PillsPerDose
    {
        get => _pillsPerDose;
        set => SetProperty(ref _pillsPerDose, value);
    }

    private bool _isActive = true;
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                ((Command)LoadCommand).ChangeCanExecute();
                ((Command)LoadPeopleCommand).ChangeCanExecute();
                ((Command)SearchCommand).ChangeCanExecute();
                ((Command)ShowLowSupplyCommand).ChangeCanExecute();
                ((Command)ShowExpiredCommand).ChangeCanExecute();
            }
        }
    }

    // Commands
    public ICommand LoadCommand { get; }
    public ICommand LoadPeopleCommand { get; }
    public ICommand SaveMedicationCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand TakeDoseCommand { get; }
    public ICommand RefillCommand { get; }
    public ICommand ShowLowSupplyCommand { get; }
    public ICommand ShowExpiredCommand { get; }
    public ICommand ShowAllCommand { get; }
    public ICommand AddScheduleTimeCommand { get; }
    public ICommand RemoveScheduleTimeCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MedicationsViewModel(DatabaseService database, MedicationActionService medicationActionService, NotificationService? notificationService = null)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _medicationActionService = medicationActionService ?? throw new ArgumentNullException(nameof(medicationActionService));
        _notificationService = notificationService;

        LoadCommand = new Command(async () => await LoadAsync(), () => !IsBusy);
        LoadPeopleCommand = new Command(async () => await LoadPeopleAsync(), () => !IsBusy);
        SaveMedicationCommand = new Command(async () => await SaveMedicationAsync(), CanSaveMedication);
        SearchCommand = new Command(async () => await SearchAsync(), () => !IsBusy);
        DeleteCommand = new Command<Medication>(async m => await DeleteMedicationAsync(m));
        TakeDoseCommand = new Command<Medication>(async m => await TakeDoseAsync(m));
        RefillCommand = new Command<Medication>(async m => await RefillAsync(m));
        ShowLowSupplyCommand = new Command(async () => await ShowLowSupplyAsync(), () => !IsBusy);
        ShowExpiredCommand = new Command(async () => await ShowExpiredAsync(), () => !IsBusy);
        ShowAllCommand = new Command(async () => await LoadAsync(), () => !IsBusy);
        AddScheduleTimeCommand = new Command(() => AddScheduleTime());
        RemoveScheduleTimeCommand = new Command<ScheduleTimeViewModel>(time => RemoveScheduleTime(time));
    }

    private bool CanSaveMedication()
        => !string.IsNullOrWhiteSpace(Name) &&
           !string.IsNullOrWhiteSpace(Dosage) &&
           SelectedPerson != null;

    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            Medications.Clear();

            var medications = await _database.GetMedicationsAsync();
            foreach (var medication in medications)
            {
                Medications.Add(medication);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadPeopleAsync()
    {
        try
        {
            IsBusy = true;
            People.Clear();

            var people = await _database.GetPeopleAsync();
            foreach (var person in people)
            {
                People.Add(person);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadMedicationsForPersonAsync(int personId)
    {
        try
        {
            IsBusy = true;
            Medications.Clear();

            var medications = await _database.GetMedicationsAsync(personId);
            foreach (var medication in medications)
            {
                Medications.Add(medication);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchAsync()
    {
        try
        {
            IsBusy = true;
            Medications.Clear();

            var personId = SelectedPerson?.Id;
            var results = await _database.SearchMedicationsAsync(SearchText, personId);
            foreach (var medication in results)
            {
                Medications.Add(medication);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ============================================================
    // ENHANCED SaveMedicationAsync with detailed error messages
    // ============================================================
    private async Task SaveMedicationAsync()
    {
        // Check basic requirements first
        if (!CanSaveMedication() || SelectedPerson == null)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Missing Information",
                "Please ensure:\n" +
                "• A person is selected\n" +
                "• Medication name is entered\n" +
                "• Dosage is entered",
                "OK");
            return;
        }

        // Check if SelectedPerson has a valid ID
        if (SelectedPerson.Id <= 0)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Error",
                $"The selected person has an invalid ID ({SelectedPerson.Id}). " +
                "Please go to the People tab and add a person first.",
                "OK");
            return;
        }

        try
        {
            if (SelectedMedication is null)
            {
                // Create new medication
                var medication = new Medication
                {
                    PersonId = SelectedPerson.Id,
                    Name = Name,
                    Dosage = Dosage,
                    Instructions = Instructions,
                    PrescribingDoctor = PrescribingDoctor,
                    Pharmacy = Pharmacy,
                    PrescriptionDate = PrescriptionDate,
                    ExpirationDate = ExpirationDate,
                    RefillsAuthorized = RefillsAuthorized,
                    RefillsRemaining = RefillsRemaining,
                    CurrentSupply = CurrentSupply,
                    LowSupplyThreshold = LowSupplyThreshold,
                    PillsPerDose = PillsPerDose,
                    IsActive = IsActive,
                    Notes = Notes
                };

                // Debug info
                System.Diagnostics.Debug.WriteLine($"Creating medication: PersonId={medication.PersonId}, Name={medication.Name}");

                // Validate before saving
                if (!medication.Validate())
                {
                    // Build detailed error message
                    var errors = new System.Collections.Generic.List<string>();

                    if (medication.PersonId <= 0)
                        errors.Add($"• PersonId is invalid ({medication.PersonId})");
                    if (string.IsNullOrWhiteSpace(medication.Name))
                        errors.Add("• Name is empty");
                    if (string.IsNullOrWhiteSpace(medication.Dosage))
                        errors.Add("• Dosage is empty");
                    if (medication.PrescriptionDate.HasValue && medication.PrescriptionDate.Value > DateTime.UtcNow)
                        errors.Add("• Prescription date is in the future");
                    if (medication.PrescriptionDate.HasValue && medication.ExpirationDate.HasValue &&
                        medication.ExpirationDate.Value < medication.PrescriptionDate.Value)
                        errors.Add("• Expiration date is before prescription date");
                    if (medication.RefillsRemaining > medication.RefillsAuthorized)
                        errors.Add($"• Refills remaining ({medication.RefillsRemaining}) exceeds authorized ({medication.RefillsAuthorized})");
                    if (medication.CurrentSupply < 0)
                        errors.Add($"• Current supply is negative ({medication.CurrentSupply})");
                    if (medication.RefillsAuthorized < 0)
                        errors.Add($"• Refills authorized is negative ({medication.RefillsAuthorized})");
                    if (medication.RefillsRemaining < 0)
                        errors.Add($"• Refills remaining is negative ({medication.RefillsRemaining})");
                    if (medication.LowSupplyThreshold < 0)
                        errors.Add($"• Low supply threshold is negative ({medication.LowSupplyThreshold})");

                    await Application.Current!.Windows[0].Page!.DisplayAlert(
                        "Validation Failed",
                        "The medication has the following errors:\n\n" + string.Join("\n", errors),
                        "OK");
                    return;
                }

                await _database.SaveMedicationAsync(medication);
                
                // Save daily schedule if times are specified
                await SaveDailyScheduleAsync(medication.Id);
                
                // Schedule notifications for this medication
                if (_notificationService != null)
                {
                    await _notificationService.ScheduleNotificationsForMedicationAsync(medication.Id);
                }
            }
            else
            {
                // Update existing medication
                SelectedMedication.PersonId = SelectedPerson.Id;
                SelectedMedication.Name = Name;
                SelectedMedication.Dosage = Dosage;
                SelectedMedication.Instructions = Instructions;
                SelectedMedication.PrescribingDoctor = PrescribingDoctor;
                SelectedMedication.Pharmacy = Pharmacy;
                SelectedMedication.PrescriptionDate = PrescriptionDate;
                SelectedMedication.ExpirationDate = ExpirationDate;
                SelectedMedication.RefillsAuthorized = RefillsAuthorized;
                SelectedMedication.RefillsRemaining = RefillsRemaining;
                SelectedMedication.CurrentSupply = CurrentSupply;
                SelectedMedication.LowSupplyThreshold = LowSupplyThreshold;
                SelectedMedication.PillsPerDose = PillsPerDose;
                SelectedMedication.IsActive = IsActive;
                SelectedMedication.Notes = Notes;

                // Validate before saving
                if (!SelectedMedication.Validate())
                {
                    await Application.Current!.Windows[0].Page!.DisplayAlert(
                        "Validation Failed",
                        "The medication failed validation. Please check all fields.",
                        "OK");
                    return;
                }

                await _database.SaveMedicationAsync(SelectedMedication);
                
                // Save daily schedule if times are specified
                await SaveDailyScheduleAsync(SelectedMedication.Id);
                
                // Schedule notifications for this medication
                if (_notificationService != null)
                {
                    await _notificationService.ScheduleNotificationsForMedicationAsync(SelectedMedication.Id);
                }
            }

            // Clear form
            ClearForm();
            OnPropertyChanged(nameof(SaveButtonText));

            // Reload medications
            await LoadAsync();

            // Success message
            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Success",
                "Medication saved successfully!",
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Error",
                $"Failed to save medication:\n\n{ex.Message}\n\nDetails: {ex.InnerException?.Message ?? "None"}",
                "OK");
        }
    }

    private async Task DeleteMedicationAsync(Medication? medication)
    {
        if (medication == null)
        {
            return;
        }

        try
        {
            await _database.SoftDeleteMedicationAsync(medication);
            Medications.Remove(medication);

            if (SelectedMedication == medication)
            {
                ClearForm();
                OnPropertyChanged(nameof(SaveButtonText));
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Error",
                $"Failed to delete medication: {ex.Message}",
                "OK");
        }
    }

    private async Task TakeDoseAsync(Medication? medication)
    {
        if (medication == null)
        {
            return;
        }

        // Store references before the async operation
        var medicationId = medication.Id;
        var wasSelected = SelectedMedication?.Id == medicationId;

        // Use the shared service to handle taking a dose
        // Refresh callback updates the specific item in the collection
        await _medicationActionService.TakeDoseAsync(medication, async () =>
        {
            // Reload from database to get latest state
            var updatedMedication = await _database.GetMedicationByIdAsync(medicationId);
            if (updatedMedication != null)
            {
                // Find the medication in the collection by ID (more reliable than reference)
                var existingMedication = Medications.FirstOrDefault(m => m.Id == medicationId);
                if (existingMedication != null)
                {
                    var index = Medications.IndexOf(existingMedication);
                    if (index >= 0)
                    {
                        Medications[index] = updatedMedication;
                    }
                }

                // If this medication was selected, update SelectedMedication to the reloaded version
                // This ensures the form fields are updated with the correct values
                if (wasSelected)
                {
                    SelectedMedication = updatedMedication;
                }
            }
        });
    }

    private async Task RefillAsync(Medication? medication)
    {
        if (medication == null)
        {
            return;
        }

        // Store the ID and whether it's selected before modifying
        var medicationId = medication.Id;
        var wasSelected = SelectedMedication?.Id == medicationId;

        await _medicationActionService.RefillAsync(medication, () =>
        {
            // Update the item in the collection with the mutated medication
            var existingMedication = Medications.FirstOrDefault(m => m.Id == medicationId);
            if (existingMedication != null)
            {
                var index = Medications.IndexOf(existingMedication);
                if (index >= 0)
                {
                    Medications[index] = medication;
                }
            }

            // If this medication was selected, update SelectedMedication
            if (wasSelected)
            {
                SelectedMedication = medication;
            }

            return Task.CompletedTask;
        });
    }

    private async Task ShowLowSupplyAsync()
    {
        try
        {
            IsBusy = true;
            Medications.Clear();

            var personId = SelectedPerson?.Id;
            var lowSupply = await _database.GetLowSupplyMedicationsAsync(personId);
            foreach (var medication in lowSupply)
            {
                Medications.Add(medication);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ShowExpiredAsync()
    {
        try
        {
            IsBusy = true;
            Medications.Clear();

            var personId = SelectedPerson?.Id;
            var expired = await _database.GetExpiredMedicationsAsync(personId);
            foreach (var medication in expired)
            {
                Medications.Add(medication);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadScheduleTimesAsync(int medicationId)
    {
        try
        {
            ScheduleTimes.Clear();
            
            var schedules = await _database.GetSchedulesForMedicationAsync(medicationId);
            var dailySchedule = schedules.OfType<DailySchedule>().FirstOrDefault();
            
            if (dailySchedule != null && !string.IsNullOrWhiteSpace(dailySchedule.TimesOfDay))
            {
                // Parse the times from the DailySchedule
                var parts = dailySchedule.TimesOfDay.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int minutes) && minutes >= 0 && minutes < 1440)
                    {
                        var timeSpan = TimeSpan.FromMinutes(minutes);
                        ScheduleTimes.Add(new ScheduleTimeViewModel(timeSpan));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // If schedule loading fails, just start with empty list
            System.Diagnostics.Debug.WriteLine($"Error loading schedule times: {ex.Message}");
            ScheduleTimes.Clear();
        }
    }

    private void AddScheduleTime()
    {
        // Add a default time (8:00 AM) if list is empty, otherwise add one hour after the last time
        TimeSpan newTime;
        if (ScheduleTimes.Count == 0)
        {
            newTime = new TimeSpan(8, 0, 0); // 8:00 AM
        }
        else
        {
            var lastTime = ScheduleTimes.OrderBy(t => t.Time).Last().Time;
            newTime = lastTime.Add(TimeSpan.FromHours(1));
            // If it goes past midnight, wrap to 8:00 AM
            if (newTime.Days > 0)
            {
                newTime = new TimeSpan(8, 0, 0);
            }
        }
        ScheduleTimes.Add(new ScheduleTimeViewModel(newTime));
    }

    private void RemoveScheduleTime(ScheduleTimeViewModel time)
    {
        ScheduleTimes.Remove(time);
    }

    private async Task SaveDailyScheduleAsync(int medicationId)
    {
        try
        {
            // Get existing schedules for this medication
            var existingSchedules = await _database.GetSchedulesForMedicationAsync(medicationId);
            var existingDailySchedule = existingSchedules.OfType<DailySchedule>().FirstOrDefault();

            if (ScheduleTimes.Count > 0)
            {
                // Convert TimeSpan values to minutes since midnight
                var minutesList = ScheduleTimes
                    .Select(st => (int)st.Time.TotalMinutes)
                    .OrderBy(m => m)
                    .ToList();

                var timesOfDay = string.Join(",", minutesList);

                DailySchedule schedule;
                if (existingDailySchedule != null)
                {
                    // Update existing schedule
                    schedule = existingDailySchedule;
                    schedule.TimesOfDay = timesOfDay;
                    schedule.IsActive = true;
                }
                else
                {
                    // Create new schedule
                    schedule = new DailySchedule
                    {
                        MedicationId = medicationId,
                        ScheduleType = "Daily",
                        TimesOfDay = timesOfDay,
                        IsActive = true
                    };
                }

                await _database.SaveScheduleAsync(schedule);
            }
            else
            {
                // If no times specified, soft-delete existing daily schedule if it exists
                if (existingDailySchedule != null)
                {
                    await _database.SoftDeleteScheduleAsync(existingDailySchedule);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the medication save
            System.Diagnostics.Debug.WriteLine($"Error saving daily schedule: {ex.Message}");
        }
    }

    private void ClearForm()
    {
        Name = string.Empty;
        Dosage = string.Empty;
        Instructions = string.Empty;
        PrescribingDoctor = string.Empty;
        Pharmacy = string.Empty;
        PrescriptionDate = null;
        ExpirationDate = null;
        RefillsAuthorized = 0;
        RefillsRemaining = 0;
        CurrentSupply = 0;
        LowSupplyThreshold = 10;
        IsActive = true;
        Notes = null;
        SelectedMedication = null;
        ScheduleTimes.Clear();
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(backingStore, value))
        {
            return false;
        }

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
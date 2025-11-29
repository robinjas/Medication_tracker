using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FMMS.Models;
using FMMS.Reports;
using FMMS.Services;
using Microsoft.Maui.Controls;

namespace FMMS.ViewModels;

/// <summary>
/// Main dashboard ViewModel for the FMMS application.
/// Provides overview statistics, unified search, and quick access to key features.
/// Demonstrates MVVM pattern and scalable design.
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _database;
    private readonly SearchService _searchService;
    private readonly ReportService _reportService;
    private readonly MedicationActionService _medicationActionService;

    // Statistics
    private int _totalMedications;
    public int TotalMedications
    {
        get => _totalMedications;
        set => SetProperty(ref _totalMedications, value);
    }

    private int _activeMedications;
    public int ActiveMedications
    {
        get => _activeMedications;
        set => SetProperty(ref _activeMedications, value);
    }

    private int _lowSupplyCount;
    public int LowSupplyCount
    {
        get => _lowSupplyCount;
        set => SetProperty(ref _lowSupplyCount, value);
    }

    private int _upcomingRefills;
    public int UpcomingRefills
    {
        get => _upcomingRefills;
        set => SetProperty(ref _upcomingRefills, value);
    }

    // Collections
    public ObservableCollection<Person> People { get; } = new();
    public ObservableCollection<Medication> RecentMedications { get; } = new();
    public ObservableCollection<Medication> LowSupplyMedications { get; } = new();
    public ObservableCollection<Medication> RefillNeededMedications { get; } = new();
    public ObservableCollection<SearchResult> SearchResults { get; } = new();

    // Search
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    SearchResults.Clear();
                }
            }
        }
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
                ((Command)SearchCommand).ChangeCanExecute();
                ((Command)GenerateReportCommand).ChangeCanExecute();
            }
        }
    }

    public ICommand LoadCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand NavigateToPeopleCommand { get; }
    public ICommand NavigateToMedicationsCommand { get; }
    public ICommand TakeDoseCommand { get; }
    public ICommand RefillCommand { get; }
    public ICommand GenerateReportCommand { get; }
    public ICommand RefreshCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel(
        DatabaseService database,
        SearchService searchService,
        ReportService reportService,
        MedicationActionService medicationActionService)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _medicationActionService = medicationActionService ?? throw new ArgumentNullException(nameof(medicationActionService));

        LoadCommand = new Command(async () => await LoadAsync(), () => !IsBusy);
        SearchCommand = new Command(async () => await SearchAsync(), () => !IsBusy && !string.IsNullOrWhiteSpace(SearchText));
        NavigateToPeopleCommand = new Command(async () => await NavigateToPeopleAsync());
        NavigateToMedicationsCommand = new Command(async () => await NavigateToMedicationsAsync());
        TakeDoseCommand = new Command<Medication>(async m => await TakeDoseAsync(m));
        RefillCommand = new Command<Medication>(async m => await RefillAsync(m));
        GenerateReportCommand = new Command(async () => await GenerateReportAsync(), () => !IsBusy);
        RefreshCommand = new Command(async () => await LoadAsync(), () => !IsBusy);
    }

    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;

            // Load people
            var people = await _database.GetPeopleAsync();
            People.Clear();
            foreach (var person in people)
            {
                People.Add(person);
            }

            // Load all medications
            var allMedications = await _database.GetMedicationsAsync(includeDeleted: false);

            // Calculate statistics
            TotalMedications = allMedications.Count;
            ActiveMedications = allMedications.Count(m => m.IsActive);
            LowSupplyCount = allMedications.Count(m => m.IsSupplyLow());
            UpcomingRefills = allMedications.Count(m => m.NeedsRefill());

            // Load recent medications (last 5 updated)
            RecentMedications.Clear();
            var recent = allMedications
                .OrderByDescending(m => m.UpdatedAt)
                .Take(5)
                .ToList();
            foreach (var med in recent)
            {
                RecentMedications.Add(med);
            }

            // Load low supply medications
            LowSupplyMedications.Clear();
            var lowSupply = await _database.GetLowSupplyMedicationsAsync();
            foreach (var med in lowSupply.Take(5))
            {
                LowSupplyMedications.Add(med);
            }

            // Load medications needing refills
            RefillNeededMedications.Clear();
            var refillNeeded = allMedications
                .Where(m => m.NeedsRefill())
                .OrderBy(m => m.CurrentSupply)
                .Take(5)
                .ToList();
            foreach (var med in refillNeeded)
            {
                RefillNeededMedications.Add(med);
            }
        }
        catch (Exception ex)
        {
            // Log error - in production would use proper logging
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
            // Don't crash the app - just leave data empty
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            SearchResults.Clear();
            return;
        }

        try
        {
            IsBusy = true;
            SearchResults.Clear();

            var results = await _searchService.SearchAllAsync(SearchText);
            foreach (var result in results)
            {
                SearchResults.Add(result);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TakeDoseAsync(Medication? medication)
    {
        if (medication == null)
        {
            return;
        }

        // Use the shared service to handle taking a dose
        // Refresh callback reloads the entire dashboard
        await _medicationActionService.TakeDoseAsync(medication, async () => await LoadAsync());
    }

    private async Task RefillAsync(Medication? medication)
    {
        if (medication == null)
        {
            return;
        }

        // Use the shared service to handle refills
        // Refresh callback reloads the entire dashboard
        await _medicationActionService.RefillAsync(medication, async () => await LoadAsync());
    }

    private async Task NavigateToPeopleAsync()
    {
        // With TabbedPage structure, navigation is handled by the tabs
        // These buttons are optional helpers - users can tap the tabs directly
        if (Application.Current?.Windows.Count > 0)
        {
            var page = Application.Current.Windows[0].Page;
            if (page is TabbedPage tabbedPage)
            {
                // Switch to People tab (index 1, since Dashboard is index 0)
                if (tabbedPage.Children.Count > 1)
                {
                    tabbedPage.CurrentPage = tabbedPage.Children[1];
                }
            }
        }
        await Task.CompletedTask;
    }

    private async Task NavigateToMedicationsAsync()
    {
        // With TabbedPage structure, navigation is handled by the tabs
        if (Application.Current?.Windows.Count > 0)
        {
            var page = Application.Current.Windows[0].Page;
            if (page is TabbedPage tabbedPage)
            {
                // Switch to Medications tab (index 2, Dashboard=0, People=1)
                if (tabbedPage.Children.Count > 2)
                {
                    tabbedPage.CurrentPage = tabbedPage.Children[2];
                }
            }
        }
        await Task.CompletedTask;
    }

    private async Task GenerateReportAsync()
    {
        try
        {
            IsBusy = true;

            var reportsDir = ReportService.GetReportsDirectory();
            var filename = ReportService.GenerateReportFilename("MedicationSummary");
            var outputPath = System.IO.Path.Combine(reportsDir, filename);

            await _reportService.GenerateMedicationSummaryReportAsync(outputPath);

            var page = Application.Current!.Windows[0].Page;
            if (page != null)
            {
                await page.DisplayAlert(
                    "Report Generated",
                    $"Summary report saved!\n\nPath: {outputPath}",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            var page = Application.Current!.Windows[0].Page;
            if (page != null)
            {
                await page.DisplayAlert(
                    "Error",
                    $"Failed to generate report:\n{ex.Message}",
                    "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
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


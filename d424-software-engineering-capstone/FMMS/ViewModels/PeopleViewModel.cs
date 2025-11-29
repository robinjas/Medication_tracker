using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FMMS.Models;
using FMMS.Reports;
using FMMS.Services;
using Microsoft.Maui.Controls;

namespace FMMS.ViewModels;

public class PeopleViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _database;
    private readonly ReportService _reportService;

    public ObservableCollection<Person> People { get; } = new();

    private Person? _selectedPerson;
    public Person? SelectedPerson
    {
        get => _selectedPerson;
        set
        {
            if (SetProperty(ref _selectedPerson, value))
            {
                if (value != null)
                {
                    FirstName = value.FirstName;
                    LastName = value.LastName;
                }

                OnPropertyChanged(nameof(SaveButtonText));
                ((Command)AddPersonCommand).ChangeCanExecute();
            }
        }
    }

    public string SaveButtonText => SelectedPerson == null ? "Add" : "Update";

    private string _firstName = string.Empty;
    public string FirstName
    {
        get => _firstName;
        set
        {
            if (SetProperty(ref _firstName, value))
            {
                ((Command)AddPersonCommand).ChangeCanExecute();
            }
        }
    }

    private string _lastName = string.Empty;
    public string LastName
    {
        get => _lastName;
        set
        {
            if (SetProperty(ref _lastName, value))
            {
                ((Command)AddPersonCommand).ChangeCanExecute();
            }
        }
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
                ((Command)SearchCommand).ChangeCanExecute();
                ((Command)GenerateReportCommand).ChangeCanExecute();
            }
        }
    }

    public ICommand LoadCommand { get; }
    public ICommand AddPersonCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand GenerateReportCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public PeopleViewModel(DatabaseService database, ReportService reportService)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));

        LoadCommand = new Command(async () => await LoadAsync(), () => !IsBusy);
        AddPersonCommand = new Command(async () => await AddOrUpdatePersonAsync(), CanAddOrUpdatePerson);
        SearchCommand = new Command(async () => await SearchAsync(), () => !IsBusy);
        DeleteCommand = new Command<Person>(async p => await DeletePersonAsync(p));
        GenerateReportCommand = new Command(async () => await GenerateReportAsync(), () => !IsBusy);
    }

    private bool CanAddOrUpdatePerson()
        => !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName);

    private async Task LoadAsync()
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

    private async Task SearchAsync()
    {
        try
        {
            IsBusy = true;
            People.Clear();
            var results = await _database.SearchPeopleAsync(SearchText);
            foreach (var person in results)
            {
                People.Add(person);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddOrUpdatePersonAsync()
    {
        if (!CanAddOrUpdatePerson())
        {
            return;
        }

        if (SelectedPerson is null)
        {
            var person = new Person
            {
                FirstName = FirstName,
                LastName = LastName
            };
            await _database.SavePersonAsync(person);
        }
        else
        {
            SelectedPerson.FirstName = FirstName;
            SelectedPerson.LastName = LastName;
            await _database.SavePersonAsync(SelectedPerson);
        }

        FirstName = string.Empty;
        LastName = string.Empty;
        SelectedPerson = null;
        OnPropertyChanged(nameof(SaveButtonText));

        await LoadAsync();
    }

    private async Task DeletePersonAsync(Person? person)
    {
        if (person == null)
        {
            return;
        }

        await _database.SoftDeletePersonAsync(person);
        People.Remove(person);

        if (SelectedPerson == person)
        {
            SelectedPerson = null;
            FirstName = string.Empty;
            LastName = string.Empty;
            OnPropertyChanged(nameof(SaveButtonText));
        }
    }

    private async Task GenerateReportAsync()
    {
        try
        {
            IsBusy = true;

            var reportsDir = ReportService.GetReportsDirectory();
            var filename = ReportService.GenerateReportFilename("PeopleSummary");
            var outputPath = Path.Combine(reportsDir, filename);

            await _reportService.GeneratePeopleSummaryReportAsync(outputPath);

            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Report Generated",
                $"Report saved!\n\nPath: {outputPath}\n\nTotal: {People.Count} people",
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Error",
                $"Failed to generate report:\n{ex.Message}",
                "OK");
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
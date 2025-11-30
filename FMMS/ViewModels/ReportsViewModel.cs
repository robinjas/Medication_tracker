using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FMMS.Reports;
using Microsoft.Maui.Controls;

namespace FMMS.ViewModels;

public class ReportsViewModel : INotifyPropertyChanged
{
    private readonly ReportService _reportService;
    private bool _isBusy;

    public ReportsViewModel(ReportService reportService)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        
        GenerateReportCommand = new Command(async () => await GenerateReportAsync(), () => !IsBusy);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                ((Command)GenerateReportCommand).ChangeCanExecute();
            }
        }
    }

    public ICommand GenerateReportCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task GenerateReportAsync()
    {
        try
        {
            IsBusy = true;

            var reportsDir = ReportService.GetReportsDirectory();
            var filename = ReportService.GenerateReportFilename("Summary");
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


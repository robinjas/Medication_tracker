using FMMS.Models;
using FMMS.ViewModels;

namespace FMMS.Views;

public partial class MedicationsListPage : ContentPage
{
    private readonly MedicationsViewModel _viewModel;

    public MedicationsListPage(MedicationsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Load medications (this will set up "All People" placeholder and show all medications)
        if (_viewModel.LoadCommand.CanExecute(null))
        {
            _viewModel.LoadCommand.Execute(null);
        }
    }

    private async void OnAddNewMedicationClicked(object? sender, EventArgs e)
    {
        // Navigate to add/edit page with no selected medication (new medication)
        _viewModel.SelectedMedication = null;
        _viewModel.ClearForm();
        
        // Create MedicationsPage with the same ViewModel instance
        // Using fully qualified name to avoid any namespace resolution issues
        var addEditPage = new FMMS.Views.MedicationsPage(_viewModel);
        await Navigation.PushAsync(addEditPage);
    }

    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Medication medication)
        {
            _viewModel.SelectedMedication = medication;
            
            // Create MedicationsPage with the same ViewModel instance
            var addEditPage = new FMMS.Views.MedicationsPage(_viewModel);
            await Navigation.PushAsync(addEditPage);
        }
    }
}


using FMMS.ViewModels;

namespace FMMS.Views;

public partial class MedicationsPage : ContentPage
{
    private readonly MedicationsViewModel _viewModel;

    public MedicationsPage(MedicationsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Subscribe to medication saved event to navigate back
        _viewModel.MedicationSaved += OnMedicationSaved;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Load people for the picker
        if (_viewModel.LoadPeopleCommand.CanExecute(null))
        {
            _viewModel.LoadPeopleCommand.Execute(null);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Unsubscribe from event
        _viewModel.MedicationSaved -= OnMedicationSaved;
    }

    private async void OnMedicationSaved(object? sender, EventArgs e)
    {
        // Navigate back to the list page after successful save
        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        // Navigate back to the list page
        await Navigation.PopAsync();
    }
}
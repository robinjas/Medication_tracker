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
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Load people for the picker
        if (_viewModel.LoadPeopleCommand.CanExecute(null))
        {
            _viewModel.LoadPeopleCommand.Execute(null);
        }

        // Load medications
        if (_viewModel.LoadCommand.CanExecute(null))
        {
            _viewModel.LoadCommand.Execute(null);
        }
    }
}
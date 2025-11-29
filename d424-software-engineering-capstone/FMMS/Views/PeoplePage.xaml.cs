using FMMS.ViewModels;

namespace FMMS.Views;

public partial class PeoplePage : ContentPage
{
    private readonly PeopleViewModel _viewModel;

    public PeoplePage(PeopleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.LoadCommand.CanExecute(null))
        {
            _viewModel.LoadCommand.Execute(null);
        }
    }
}

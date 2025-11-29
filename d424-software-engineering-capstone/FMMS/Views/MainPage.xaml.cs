using FMMS.ViewModels;

namespace FMMS.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        try
        {
            InitializeComponent();
            
            if (viewModel == null)
            {
                System.Diagnostics.Debug.WriteLine("MainViewModel is null!");
                return;
            }
            
            BindingContext = viewModel;

            // Set current date - check if DateLabel exists first
            if (DateLabel != null)
            {
                DateLabel.Text = DateTime.Now.ToString("MMMM dd, yyyy");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in MainPage constructor: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw; // Re-throw to see the actual error
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Refresh data when page appears
        if (BindingContext is MainViewModel viewModel && viewModel.LoadCommand.CanExecute(null))
        {
            // Execute command - it's already async internally
            viewModel.LoadCommand.Execute(null);
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        // Auto-search as user types (with debounce in real app)
        if (BindingContext is MainViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                viewModel.SearchResults.Clear();
            }
        }
    }
}


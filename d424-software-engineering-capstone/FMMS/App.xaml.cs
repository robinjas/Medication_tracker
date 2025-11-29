using FMMS.Views;

namespace FMMS;

/// <summary>
/// The main application class.
/// Uses CreateWindow to create a TabbedPage with MainPage (Dashboard), PeoplePage, and MedicationsPage.
/// </summary>
public partial class App : Application
{
    private readonly MainPage _mainPage;
    private readonly PeoplePage _peoplePage;
    private readonly MedicationsPage _medicationsPage;

    public App(MainPage mainPage, PeoplePage peoplePage, MedicationsPage medicationsPage)
    {
        InitializeComponent();

        _mainPage = mainPage ?? throw new ArgumentNullException(nameof(mainPage));
        _peoplePage = peoplePage ?? throw new ArgumentNullException(nameof(peoplePage));
        _medicationsPage = medicationsPage ?? throw new ArgumentNullException(nameof(medicationsPage));
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Create tabbed page with all three pages
        var tabbedPage = new TabbedPage();

        // Add Dashboard tab
        tabbedPage.Children.Add(new NavigationPage(_mainPage)
        {
            Title = "Dashboard",
            IconImageSource = "home.png"
        });

        tabbedPage.Children.Add(new NavigationPage(_peoplePage)
        {
            Title = "People",
            IconImageSource = "people.png"
        });

        tabbedPage.Children.Add(new NavigationPage(_medicationsPage)
        {
            Title = "Medications",
            IconImageSource = "pill.png"
        });

        // Style the tabbed page
        tabbedPage.BarBackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#4A90E2");
        tabbedPage.BarTextColor = Microsoft.Maui.Graphics.Colors.White;
        tabbedPage.SelectedTabColor = Microsoft.Maui.Graphics.Colors.White;
        tabbedPage.UnselectedTabColor = Microsoft.Maui.Graphics.Color.FromArgb("#FFFFFF80");

        return new Window(tabbedPage);
    }
}
using FMMS.Views;

namespace FMMS;

/// <summary>
/// The main application class.
/// Uses CreateWindow to create a TabbedPage with MainPage (Dashboard), PeoplePage, and MedicationsListPage.
/// </summary>
public partial class App : Application
{
    private readonly MainPage _mainPage;
    private readonly PeoplePage _peoplePage;
    private readonly MedicationsListPage _medicationsListPage;
    private readonly ReportsPage _reportsPage;

    public App(MainPage mainPage, PeoplePage peoplePage, MedicationsListPage medicationsListPage, ReportsPage reportsPage)
    {
        InitializeComponent();

        _mainPage = mainPage ?? throw new ArgumentNullException(nameof(mainPage));
        _peoplePage = peoplePage ?? throw new ArgumentNullException(nameof(peoplePage));
        _medicationsListPage = medicationsListPage ?? throw new ArgumentNullException(nameof(medicationsListPage));
        _reportsPage = reportsPage ?? throw new ArgumentNullException(nameof(reportsPage));
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

        tabbedPage.Children.Add(new NavigationPage(_medicationsListPage)
        {
            Title = "Medications",
            IconImageSource = "pill.png"
        });

        tabbedPage.Children.Add(new NavigationPage(_reportsPage)
        {
            Title = "Reports",
            IconImageSource = "report.png"
        });

        // Style the tabbed page
        tabbedPage.BarBackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#4A90E2");
        tabbedPage.BarTextColor = Microsoft.Maui.Graphics.Colors.White;
        tabbedPage.SelectedTabColor = Microsoft.Maui.Graphics.Colors.White;
        tabbedPage.UnselectedTabColor = Microsoft.Maui.Graphics.Color.FromArgb("#FFFFFF80");

        return new Window(tabbedPage);
    }
}
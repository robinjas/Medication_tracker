using System.IO;
using FMMS.Reports;
using FMMS.Services;
using FMMS.ViewModels;
using FMMS.Views;
using Microsoft.Maui.Storage;

namespace FMMS;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Database path
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "fmms.db");

        // Services (Singletons)
        builder.Services.AddSingleton(new DatabaseService(dbPath));
        builder.Services.AddSingleton<ReportService>(sp =>
            new ReportService(sp.GetRequiredService<DatabaseService>()));
        builder.Services.AddSingleton<SearchService>(sp =>
            new SearchService(sp.GetRequiredService<DatabaseService>()));
        builder.Services.AddSingleton<MedicationActionService>(sp =>
            new MedicationActionService(sp.GetRequiredService<DatabaseService>()));
        builder.Services.AddSingleton<NotificationService>(sp =>
            new NotificationService(sp.GetRequiredService<DatabaseService>()));

        // ViewModels (Transient)
        builder.Services.AddTransient<MainViewModel>(sp =>
            new MainViewModel(
                sp.GetRequiredService<DatabaseService>(),
                sp.GetRequiredService<SearchService>(),
                sp.GetRequiredService<ReportService>(),
                sp.GetRequiredService<MedicationActionService>()));
        builder.Services.AddTransient<PeopleViewModel>(sp =>
            new PeopleViewModel(
                sp.GetRequiredService<DatabaseService>(),
                sp.GetRequiredService<ReportService>()));
        builder.Services.AddTransient<MedicationsViewModel>(sp =>
            new MedicationsViewModel(
                sp.GetRequiredService<DatabaseService>(),
                sp.GetRequiredService<MedicationActionService>(),
                sp.GetService<NotificationService>()));

        // Pages (Transient)
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<PeoplePage>();
        builder.Services.AddTransient<MedicationsPage>();

        var app = builder.Build();

        // Start notification service after app is built
        var notificationService = app.Services.GetService<NotificationService>();
        notificationService?.Start();

        return app;
    }
}

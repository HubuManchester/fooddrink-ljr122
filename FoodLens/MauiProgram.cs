using CommunityToolkit.Maui;
using FoodLens.Services;
using FoodLens.ViewModels;
using FoodLens.Views;
using Microsoft.Extensions.Logging;

namespace FoodLens;

/// <summary>
/// Application entry point. Configures dependency injection, services, and pages.
/// </summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Register Services (Singleton - shared instance across app)
        builder.Services.AddSingleton<RecipeService>();

        // Register ViewModels
        builder.Services.AddSingleton<RecipesViewModel>();
        builder.Services.AddTransient<RecipeDetailViewModel>();
        builder.Services.AddTransient<CameraViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();

        // Register Pages
        builder.Services.AddSingleton<RecipesPage>();
        builder.Services.AddTransient<RecipeDetailPage>();
        builder.Services.AddTransient<CameraPage>();
        builder.Services.AddSingleton<MapPage>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddTransient<HelpPage>();

        return builder.Build();
    }
}
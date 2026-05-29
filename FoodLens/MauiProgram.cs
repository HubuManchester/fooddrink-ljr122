using CommunityToolkit.Maui;
using FoodLens.Services;
using FoodLens.ViewModels;
using FoodLens.Views;
using Microsoft.Extensions.Logging;

namespace FoodLens;

/// <summary>
/// Application entry point. Configures dependency injection, services, and pages.
/// All services, view models, and pages are registered here following the
/// dependency injection pattern to promote loose coupling and testability.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the MAUI application with all required services.
    /// </summary>
    /// <returns>The configured <see cref="MauiApp"/> instance.</returns>
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

        // Register typed HttpClient for NutritionApiService (NETWORKING FEATURE).
        // AddHttpClient<T> uses IHttpClientFactory under the hood, which manages
        // HttpClient lifetimes correctly and avoids socket exhaustion issues.
        builder.Services.AddHttpClient<NutritionApiService>();

        // Register Services (Singleton — single shared instance across app lifetime)
        builder.Services.AddSingleton<RecipeService>();

        // Register ViewModels
        // Singleton: shared state (recipe list, settings) persists across navigation
        // Transient: fresh instance per navigation to avoid stale data on detail/camera pages
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
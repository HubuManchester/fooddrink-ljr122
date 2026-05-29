using FoodLens.Helpers;
using FoodLens.Models;
using FoodLens.Services;

namespace FoodLens.Views;

/// <summary>
/// Code-behind for the Map page.
/// Demonstrates Geolocation hardware feature for getting user's location
/// and displaying recipe origins on a map.
/// HARDWARE FEATURES: Geolocation/GPS, Haptic Feedback, Vibration.
/// </summary>
public partial class MapPage : ContentPage
{
    private readonly RecipeService _recipeService;

    /// <summary>
    /// Initialises the MapPage with the injected recipe service.
    /// </summary>
    /// <param name="recipeService">Service for loading recipe data.</param>
    public MapPage(RecipeService recipeService)
    {
        InitializeComponent();
        _recipeService = recipeService;
        LoadRecipeOrigins();
    }

    /// <summary>
    /// Loads recipe data to display origins in the list.
    /// </summary>
    private async void LoadRecipeOrigins()
    {
        try
        {
            var recipes = await _recipeService.GetRecipesAsync();
            OriginsCollection.ItemsSource = recipes;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error",
                $"Unable to load recipe origins: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[MapPage] Load error: {ex}");
        }
    }

    /// <summary>
    /// Gets the user's current geographic location using GPS hardware.
    /// HARDWARE FEATURE: Geolocation/GPS.
    /// </summary>
    private async void OnGetLocationClicked(object? sender, EventArgs e)
    {
        try
        {
            // Haptic feedback on button press (HARDWARE: Haptic Feedback)
            HardwareHelper.PerformHaptic(HapticFeedbackType.Click);

            LocationLabel.Text = "Getting your location...";

            // Request geolocation - HARDWARE FEATURE: GPS
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location is not null)
            {
                LocationLabel.Text = $"Latitude: {location.Latitude:F4}\n" +
                                    $"Longitude: {location.Longitude:F4}\n" +
                                    $"Altitude: {location.Altitude:F1}m";

                // Vibration to confirm location acquired (HARDWARE: Vibration)
                HardwareHelper.Vibrate(200);
            }
            else
            {
                LocationLabel.Text = "Unable to determine your location. Please ensure GPS is enabled.";
            }
        }
        catch (FeatureNotSupportedException)
        {
            LocationLabel.Text = "Geolocation is not supported on this device.";
            await DisplayAlert("Not Supported",
                "GPS/Geolocation is not available on this device.", "OK");
        }
        catch (PermissionException)
        {
            LocationLabel.Text = "Location permission was denied.";
            await DisplayAlert("Permission Required",
                "Location permission is needed. Please enable it in device settings.", "OK");
        }
        catch (Exception ex)
        {
            LocationLabel.Text = $"Error: {ex.Message}";
            await DisplayAlert("Location Error",
                $"An error occurred: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[MapPage] Geolocation error: {ex}");
        }
    }

    /// <summary>
    /// Opens the device's default map application showing a recipe's origin location.
    /// Uses platform map integration.
    /// </summary>
    private async void OnOpenMapClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.CommandParameter is Recipe recipe)
            {
                // Haptic feedback (HARDWARE: Haptic Feedback)
                HardwareHelper.PerformHaptic(HapticFeedbackType.Click);

                var location = new Location(recipe.Latitude, recipe.Longitude);
                var options = new MapLaunchOptions
                {
                    Name = $"{recipe.Name} - {recipe.Origin}"
                };

                await Map.Default.OpenAsync(location, options);
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Not Supported",
                "Map functionality is not available on this device.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Map Error",
                $"Unable to open map: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[MapPage] Map error: {ex}");
        }
    }
}
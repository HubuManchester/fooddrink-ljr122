using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodLens.Models;
using static Android.Icu.Text.CaseMap;

namespace FoodLens.ViewModels;

/// <summary>
/// ViewModel for recipe detail page.
/// Handles text-to-speech reading and map navigation.
/// </summary>
[QueryProperty(nameof(Recipe), "Recipe")]
public partial class RecipeDetailViewModel : BaseViewModel
{
    /// <summary>The recipe being displayed.</summary>
    [ObservableProperty]
    private Recipe _recipe = new();

    /// <summary>Whether TTS is currently speaking.</summary>
    [ObservableProperty]
    private bool _isSpeaking;

    /// <summary>Button text that changes based on speaking state.</summary>
    [ObservableProperty]
    private string _speakButtonText = "🔊 Read Steps Aloud";

    private CancellationTokenSource? _speechCts;

    public RecipeDetailViewModel()
    {
        Title = "Recipe Details";
    }

    /// <summary>
    /// Uses Text-to-Speech hardware to read recipe steps aloud.
    /// Toggles between speaking and stopping.
    /// </summary>
    [RelayCommand]
    private async Task ToggleSpeakStepsAsync()
    {
        if (IsSpeaking)
        {
            StopSpeaking();
            return;
        }

        if (Recipe?.Steps is null || Recipe.Steps.Count == 0)
        {
            await Shell.Current.DisplayAlert("No Steps",
                "This recipe has no steps to read aloud.", "OK");
            return;
        }

        try
        {
            IsSpeaking = true;
            SpeakButtonText = "⏹️ Stop Reading";

            // Haptic feedback to confirm action started
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);

            _speechCts = new CancellationTokenSource();

            // Build full text with step numbers for clarity
            string fullText = $"Recipe: {Recipe.Name}. ";
            fullText += $"Preparation time: {Recipe.PrepTimeMinutes} minutes. ";
            fullText += $"Cooking time: {Recipe.CookTimeMinutes} minutes. ";
            fullText += "Here are the steps: ";

            for (int i = 0; i < Recipe.Steps.Count; i++)
            {
                fullText += $"Step {i + 1}: {Recipe.Steps[i]} ";
            }

            fullText += "Recipe complete. Enjoy your meal!";

            var options = new SpeechOptions
            {
                Pitch = 1.0f,
                Volume = 0.8f
            };

            await TextToSpeech.Default.SpeakAsync(fullText, options, _speechCts.Token);
        }
        catch (OperationCanceledException)
        {
            // User cancelled - expected behaviour
            System.Diagnostics.Debug.WriteLine("[RecipeDetailVM] Speech cancelled by user.");
        }
        catch (FeatureNotSupportedException)
        {
            await Shell.Current.DisplayAlert("Not Supported",
                "Text-to-speech is not available on this device.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Speech Error",
                $"Unable to read steps: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailVM] TTS error: {ex}");
        }
        finally
        {
            IsSpeaking = false;
            SpeakButtonText = "🔊 Read Steps Aloud";
            _speechCts?.Dispose();
            _speechCts = null;
        }
    }

    /// <summary>
    /// Stops current text-to-speech playback.
    /// </summary>
    private void StopSpeaking()
    {
        if (_speechCts is not null && !_speechCts.IsCancellationRequested)
        {
            _speechCts.Cancel();
        }
    }

    /// <summary>
    /// Opens the map showing the recipe's country of origin.
    /// Uses geolocation/geocoding hardware feature.
    /// </summary>
    [RelayCommand]
    private async Task OpenMapAsync()
    {
        if (Recipe is null) return;

        try
        {
            // Haptic feedback
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);

            var location = new Location(Recipe.Latitude, Recipe.Longitude);
            var options = new MapLaunchOptions
            {
                Name = $"{Recipe.Name} - {Recipe.Origin}"
            };

            await Map.Default.OpenAsync(location, options);
        }
        catch (FeatureNotSupportedException)
        {
            await Shell.Current.DisplayAlert("Not Supported",
                "Map functionality is not available on this device.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Map Error",
                $"Unable to open map: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailVM] Map error: {ex}");
        }
    }

    /// <summary>
    /// Shares the recipe via the device's share functionality.
    /// </summary>
    [RelayCommand]
    private async Task ShareRecipeAsync()
    {
        if (Recipe is null) return;

        try
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = Recipe.Name,
                Text = $"Check out this recipe: {Recipe.Name}\n\n{Recipe.Description}\n\nOrigin: {Recipe.Origin}"
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Share Error",
                $"Unable to share recipe: {ex.Message}", "OK");
        }
    }
}
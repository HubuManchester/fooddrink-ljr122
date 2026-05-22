using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using static Android.Icu.Text.CaseMap;

namespace FoodLens.ViewModels;

/// <summary>
/// ViewModel for the Settings page.
/// Handles theme switching (dark mode), font size adjustment,
/// and other accessibility preferences.
/// These features directly address WCAG accessibility requirements.
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    /// <summary>Whether dark mode is currently enabled.</summary>
    [ObservableProperty]
    private bool _isDarkMode;

    /// <summary>Current font size multiplier for accessibility.</summary>
    [ObservableProperty]
    private double _fontSizeMultiplier = 1.0;

    /// <summary>Display text showing current font size setting.</summary>
    [ObservableProperty]
    private string _fontSizeLabel = "Font Size: Normal";

    /// <summary>Whether high contrast mode is enabled.</summary>
    [ObservableProperty]
    private bool _isHighContrast;

    /// <summary>Selected theme option name.</summary>
    [ObservableProperty]
    private string _selectedTheme = "System";

    /// <summary>Available theme options.</summary>
    public List<string> ThemeOptions { get; } = new() { "System", "Light", "Dark" };

    public SettingsViewModel()
    {
        Title = "Settings";
        LoadPreferences();
    }

    /// <summary>
    /// Loads saved user preferences from device storage.
    /// </summary>
    private void LoadPreferences()
    {
        try
        {
            SelectedTheme = Preferences.Get("AppTheme", "System");
            IsDarkMode = SelectedTheme == "Dark";
            FontSizeMultiplier = Preferences.Get("FontSizeMultiplier", 1.0);
            IsHighContrast = Preferences.Get("HighContrast", false);
            UpdateFontSizeLabel();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsVM] Error loading preferences: {ex}");
            // Use defaults if preferences cannot be loaded
        }
    }

    /// <summary>
    /// Toggles dark mode on/off and saves the preference.
    /// Addresses WCAG 1.4.3 Contrast and user preference for dark themes.
    /// </summary>
    partial void OnIsDarkModeChanged(bool value)
    {
        try
        {
            string theme = value ? "Dark" : "Light";
            SelectedTheme = theme;
            App.SetTheme(theme);
            Preferences.Set("AppTheme", theme);

            // Haptic feedback for toggle
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (FeatureNotSupportedException) { }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsVM] Theme change error: {ex}");
        }
    }

    /// <summary>
    /// Increases font size for better readability.
    /// Addresses WCAG 1.4.4 Resize Text requirement.
    /// </summary>
    [RelayCommand]
    private void IncreaseFontSize()
    {
        if (FontSizeMultiplier >= 2.0)
        {
            // Maximum reached - inform user
            Shell.Current.DisplayAlert("Maximum Size",
                "Font size is already at maximum.", "OK");
            return;
        }

        FontSizeMultiplier += 0.25;
        Preferences.Set("FontSizeMultiplier", FontSizeMultiplier);
        UpdateFontSizeLabel();

        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); }
        catch (FeatureNotSupportedException) { }
    }

    /// <summary>
    /// Decreases font size.
    /// </summary>
    [RelayCommand]
    private void DecreaseFontSize()
    {
        if (FontSizeMultiplier <= 0.75)
        {
            Shell.Current.DisplayAlert("Minimum Size",
                "Font size is already at minimum.", "OK");
            return;
        }

        FontSizeMultiplier -= 0.25;
        Preferences.Set("FontSizeMultiplier", FontSizeMultiplier);
        UpdateFontSizeLabel();

        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); }
        catch (FeatureNotSupportedException) { }
    }

    /// <summary>
    /// Resets all settings to default values.
    /// </summary>
    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert("Reset Settings",
            "Are you sure you want to reset all settings to default?",
            "Yes", "No");

        if (!confirm) return;

        try
        {
            IsDarkMode = false;
            FontSizeMultiplier = 1.0;
            IsHighContrast = false;
            SelectedTheme = "System";

            App.SetTheme("System");
            Preferences.Set("FontSizeMultiplier", 1.0);
            Preferences.Set("HighContrast", false);
            UpdateFontSizeLabel();

            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error",
                $"Unable to reset settings: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[SettingsVM] Reset error: {ex}");
        }
    }

    /// <summary>
    /// Updates the font size display label based on current multiplier.
    /// </summary>
    private void UpdateFontSizeLabel()
    {
        FontSizeLabel = FontSizeMultiplier switch
        {
            <= 0.75 => "Font Size: Small",
            <= 1.0 => "Font Size: Normal",
            <= 1.25 => "Font Size: Large",
            <= 1.5 => "Font Size: Extra Large",
            _ => "Font Size: Maximum"
        };
    }
}
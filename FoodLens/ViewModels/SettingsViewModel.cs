using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodLens.Helpers;

namespace FoodLens.ViewModels;

/// <summary>
/// ViewModel for the Settings page.
/// Handles theme switching (dark mode), font size adjustment,
/// high contrast mode, and other accessibility preferences.
/// These features directly address WCAG 2.1 accessibility requirements:
/// - WCAG 1.4.3 Contrast (Minimum): High contrast mode
/// - WCAG 1.4.4 Resize Text: Font size scaling up to 200%
/// - WCAG 1.4.6 Contrast (Enhanced): High contrast exceeds 7:1 ratio
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    /// <summary>Minimum allowed font size multiplier (75% of base).</summary>
    private const double MinFontMultiplier = 0.75;

    /// <summary>Maximum allowed font size multiplier (200% of base, WCAG 1.4.4).</summary>
    private const double MaxFontMultiplier = 2.0;

    /// <summary>Step size for font size adjustments.</summary>
    private const double FontStepSize = 0.25;

    /// <summary>Whether dark mode is currently enabled.</summary>
    [ObservableProperty]
    private bool _isDarkMode;

    /// <summary>Current font size multiplier for accessibility (WCAG 1.4.4).</summary>
    [ObservableProperty]
    private double _fontSizeMultiplier = 1.0;

    /// <summary>Display text showing current font size setting.</summary>
    [ObservableProperty]
    private string _fontSizeLabel = "Font Size: Normal (100%)";

    /// <summary>Whether high contrast mode is enabled (WCAG 1.4.6).</summary>
    [ObservableProperty]
    private bool _isHighContrast;

    /// <summary>Selected theme option name.</summary>
    [ObservableProperty]
    private string _selectedTheme = "System";

    /// <summary>Available theme options.</summary>
    public List<string> ThemeOptions { get; } = new() { "System", "Light", "Dark" };

    /// <summary>
    /// Initialises the SettingsViewModel and loads saved preferences.
    /// </summary>
    public SettingsViewModel()
    {
        Title = "Settings";
        LoadPreferences();
    }

    /// <summary>
    /// Loads saved user preferences from device storage.
    /// Uses Preferences API for persistent key-value storage.
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
        }
    }

    /// <summary>
    /// Toggles dark mode on/off and saves the preference.
    /// Addresses WCAG 1.4.3 Contrast and user preference for dark themes.
    /// </summary>
    /// <param name="value">Whether dark mode is now enabled.</param>
    partial void OnIsDarkModeChanged(bool value)
    {
        try
        {
            string theme = value ? "Dark" : "Light";

            // Only update SelectedTheme if it differs to avoid recursive notifications
            if (SelectedTheme != theme)
            {
                SelectedTheme = theme;
            }

            App.SetTheme(theme);
            Preferences.Set("AppTheme", theme);

            // Haptic feedback for toggle (HARDWARE: Haptic Feedback)
            HardwareHelper.PerformHaptic(HapticFeedbackType.Click);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsVM] Theme change error: {ex}");
        }
    }

    /// <summary>
    /// Applies high contrast mode when the toggle changes.
    /// High contrast uses WCAG AAA 7:1 contrast ratio colours for users with low vision.
    /// This addresses WCAG 1.4.6 Contrast (Enhanced).
    /// Calls <see cref="App.ApplyHighContrast"/> which updates the app-level
    /// ResourceDictionary colours, causing all DynamicResource bindings to refresh.
    /// </summary>
    /// <param name="value">Whether high contrast is now enabled.</param>
    partial void OnIsHighContrastChanged(bool value)
    {
        try
        {
            // Apply high contrast colours to the entire application
            App.ApplyHighContrast(value);
            Preferences.Set("HighContrast", value);

            // Haptic feedback for toggle (HARDWARE: Haptic Feedback)
            HardwareHelper.PerformHaptic(HapticFeedbackType.Click);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsVM] High contrast error: {ex}");
        }
    }

    /// <summary>
    /// Applies the font size multiplier to the application when it changes.
    /// Called automatically by the source generator when FontSizeMultiplier is set.
    /// Calls <see cref="App.ApplyFontSize"/> which updates the app-level
    /// ResourceDictionary font sizes, causing all DynamicResource bindings to refresh.
    /// Addresses WCAG 1.4.4 Resize Text — text can be resized up to 200%.
    /// </summary>
    /// <param name="value">The new font size multiplier value.</param>
    partial void OnFontSizeMultiplierChanged(double value)
    {
        try
        {
            App.ApplyFontSize(value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsVM] Font size apply error: {ex}");
        }
    }

    /// <summary>
    /// Increases font size for better readability.
    /// Addresses WCAG 1.4.4 Resize Text requirement.
    /// </summary>
    [RelayCommand]
    private async Task IncreaseFontSizeAsync()
    {
        await AdjustFontSizeAsync(FontStepSize, MaxFontMultiplier, "Maximum Size",
            "Font size is already at maximum (200%).");
    }

    /// <summary>
    /// Decreases font size.
    /// </summary>
    [RelayCommand]
    private async Task DecreaseFontSizeAsync()
    {
        await AdjustFontSizeAsync(-FontStepSize, MinFontMultiplier, "Minimum Size",
            "Font size is already at minimum (75%).");
    }

    /// <summary>
    /// Adjusts font size by the specified step, clamped to the given limit.
    /// Extracted to follow DRY principle — IncreaseFontSizeAsync and DecreaseFontSizeAsync
    /// previously contained near-identical logic.
    /// </summary>
    /// <param name="step">The amount to adjust (positive to increase, negative to decrease).</param>
    /// <param name="limit">The boundary value that triggers the limit alert.</param>
    /// <param name="alertTitle">Title for the limit-reached alert.</param>
    /// <param name="alertMessage">Message for the limit-reached alert.</param>
    private async Task AdjustFontSizeAsync(double step, double limit, string alertTitle, string alertMessage)
    {
        // Check if already at the limit
        bool atLimit = step > 0
            ? FontSizeMultiplier >= limit
            : FontSizeMultiplier <= limit;

        if (atLimit)
        {
            await Shell.Current.DisplayAlert(alertTitle, alertMessage, "OK");
            return;
        }

        FontSizeMultiplier += step;
        Preferences.Set("FontSizeMultiplier", FontSizeMultiplier);
        UpdateFontSizeLabel();

        // Haptic feedback for adjustment (HARDWARE: Haptic Feedback)
        HardwareHelper.PerformHaptic(HapticFeedbackType.Click);
    }

    /// <summary>
    /// Resets all settings to default values.
    /// Asks for user confirmation before proceeding (destructive action).
    /// </summary>
    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert("Reset Settings",
            "Are you sure you want to reset all settings to default? " +
            "This will restore the original theme, font size, and contrast settings.",
            "Yes", "No");

        if (!confirm)
        {
            return;
        }

        try
        {
            // Reset high contrast first (before changing other properties)
            IsHighContrast = false;

            IsDarkMode = false;
            FontSizeMultiplier = 1.0;
            SelectedTheme = "System";

            App.SetTheme("System");
            Preferences.Set("FontSizeMultiplier", 1.0);
            Preferences.Set("HighContrast", false);
            UpdateFontSizeLabel();

            // Vibration to confirm reset (HARDWARE: Vibration)
            HardwareHelper.Vibrate(200);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error",
                $"Unable to reset settings: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[SettingsVM] Reset error: {ex}");
        }
    }

    /// <summary>
    /// Updates the font size display label based on current multiplier value.
    /// Shows both the descriptive name and percentage for clarity.
    /// </summary>
    private void UpdateFontSizeLabel()
    {
        int percentage = (int)(FontSizeMultiplier * 100);

        FontSizeLabel = FontSizeMultiplier switch
        {
            <= 0.75 => $"Font Size: Small ({percentage}%)",
            <= 1.0 => $"Font Size: Normal ({percentage}%)",
            <= 1.25 => $"Font Size: Large ({percentage}%)",
            <= 1.5 => $"Font Size: Extra Large ({percentage}%)",
            _ => $"Font Size: Maximum ({percentage}%)"
        };
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodLens.Helpers;

namespace FoodLens.ViewModels;

/// <summary>
/// ViewModel for the Settings page.
/// Handles theme switching, font size adjustment, and high contrast mode.
/// Addresses WCAG 2.1 requirements:
/// - 1.4.3 Contrast (Minimum): theme and high contrast support
/// - 1.4.4 Resize Text: font size scaling 75% to 200%
/// - 1.4.6 Contrast (Enhanced): high contrast exceeds 7:1 ratio
///
/// Theme flow:
///   Picker → OnSelectedThemeChanged → App.SetTheme → ApplyColours (WCAG AA/AAA)
///   Toggle → OnIsDarkModeChanged → syncs SelectedTheme → App.SetTheme
/// Both entry points converge on App.SetTheme so colours are always consistent.
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    /// <summary>Minimum allowed font size multiplier (75% of base).</summary>
    private const double MinFontMultiplier = 0.75;

    /// <summary>Maximum allowed font size multiplier (200% of base, WCAG 1.4.4).</summary>
    private const double MaxFontMultiplier = 2.0;

    /// <summary>Step size for font size adjustments.</summary>
    private const double FontStepSize = 0.25;

    /// <summary>
    /// Guard flag to prevent the SelectedTheme and IsDarkMode partial methods
    /// from calling each other in a re-entrant loop when one syncs the other.
    /// </summary>
    private bool _isSyncingTheme;

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

    /// <summary>
    /// Selected theme option name ("System", "Light", or "Dark").
    /// Changing this via the Picker triggers <see cref="OnSelectedThemeChanged(string)"/>
    /// which applies the theme and keeps <see cref="IsDarkMode"/> in sync.
    ///
    /// FIX (Roslyn CS0419): The original cref="OnSelectedThemeChanged" was ambiguous because
    /// the CommunityToolkit.Mvvm source generator also emits an overload with signature
    /// (string? oldValue, string newValue). The cref is now disambiguated by specifying
    /// the exact single-parameter overload signature to silence CS0419.
    /// </summary>
    [ObservableProperty]
    private string _selectedTheme = "System";

    /// <summary>Available theme options displayed in the Picker.</summary>
    // FIX (Roslyn IDE0028): Use collection expression [] instead of new List<string>().
    public List<string> ThemeOptions { get; } = ["System", "Light", "Dark"];

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
    /// </summary>
    private void LoadPreferences()
    {
        try
        {
            // Temporarily suppress theme-sync callbacks while setting initial values
            _isSyncingTheme = true;

            SelectedTheme = Preferences.Get("AppTheme", "System");
            IsDarkMode = SelectedTheme == "Dark";
            FontSizeMultiplier = Preferences.Get("FontSizeMultiplier", 1.0);
            IsHighContrast = Preferences.Get("HighContrast", false);
            UpdateFontSizeLabel();

            _isSyncingTheme = false;
        }
        catch (Exception ex)
        {
            _isSyncingTheme = false;
            System.Diagnostics.Debug.WriteLine($"[SettingsVM] Error loading preferences: {ex}");
        }
    }

    /// <summary>
    /// Handles changes to <see cref="_selectedTheme"/> (e.g. from the Picker).
    /// Applies the new theme and keeps <see cref="IsDarkMode"/> in sync so that
    /// the toggle and the Picker always reflect the same state.
    ///
    /// FIX (Roslyn CS0419): This partial method corresponds to the single-parameter
    /// overload <c>OnSelectedThemeChanged(string value)</c>. The CommunityToolkit.Mvvm
    /// source generator also emits a two-parameter overload
    /// <c>OnSelectedThemeChanged(string? oldValue, string newValue)</c>.
    /// Using the single-parameter version here is deliberate — we only need the new value,
    /// and specifying the full signature in the XML doc comment resolves the ambiguity warning.
    /// </summary>
    /// <param name="value">The newly selected theme name.</param>
    partial void OnSelectedThemeChanged(string value)
    {
        if (_isSyncingTheme)
        {
            return;
        }

        try
        {
            _isSyncingTheme = true;

            // Apply the theme via the centralised App method
            App.SetTheme(value);
            Preferences.Set("AppTheme", value);

            // Keep the IsDarkMode toggle in sync with the Picker selection
            IsDarkMode = value == "Dark";

            // Screen Reader Announcement: inform accessibility users the theme changed
            SemanticScreenReader.Default.Announce($"Theme changed to {value} mode.");

            HardwareHelper.PerformHaptic(HapticFeedbackType.Click);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsVM] Theme picker error: {ex}");
        }
        finally
        {
            _isSyncingTheme = false;
        }
    }

    /// <summary>
    /// Toggles dark mode on/off. Calls App.SetTheme which internally
    /// calls ApplyColours() to ensure text remains readable in both modes.
    /// Also keeps <see cref="_selectedTheme"/> in sync so the Picker reflects the change.
    /// </summary>
    /// <param name="value">Whether dark mode is now enabled.</param>
    partial void OnIsDarkModeChanged(bool value)
    {
        if (_isSyncingTheme)
        {
            return;
        }

        try
        {
            _isSyncingTheme = true;

            string theme = value ? "Dark" : "Light";

            // Sync the Picker selection to match the toggle
            if (SelectedTheme != theme)
            {
                SelectedTheme = theme;
            }

            // SetTheme internally calls ApplyColours() which updates all semantic colours
            App.SetTheme(theme);
            Preferences.Set("AppTheme", theme);

            // Screen Reader Announcement: inform accessibility users of dark mode state
            string announcement = value ? "Dark mode enabled." : "Light mode enabled.";
            SemanticScreenReader.Default.Announce(announcement);

            HardwareHelper.PerformHaptic(HapticFeedbackType.Click);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsVM] Theme change error: {ex}");
        }
        finally
        {
            _isSyncingTheme = false;
        }
    }

    /// <summary>
    /// Applies high contrast mode. Calls App.ApplyHighContrast which internally
    /// calls ApplyColours() to update all semantic colours for the current theme.
    /// </summary>
    /// <param name="value">Whether high contrast is now enabled.</param>
    partial void OnIsHighContrastChanged(bool value)
    {
        try
        {
            // ApplyHighContrast internally calls ApplyColours()
            App.ApplyHighContrast(value);
            Preferences.Set("HighContrast", value);

            // Screen Reader Announcement: inform accessibility users of contrast change
            string announcement = value
                ? "High contrast mode enabled. Colours meet WCAG AAA 7 to 1 ratio."
                : "High contrast mode disabled. Standard contrast mode active.";
            SemanticScreenReader.Default.Announce(announcement);

            HardwareHelper.PerformHaptic(HapticFeedbackType.Click);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsVM] High contrast error: {ex}");
        }
    }

    /// <summary>
    /// Applies the font size multiplier when it changes.
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
    /// Increases font size for better readability (WCAG 1.4.4).
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
    /// Extracted to follow DRY principle — both increase and decrease share
    /// identical structure, so one method handles both via parameters.
    /// </summary>
    /// <param name="step">The amount to adjust (positive or negative).</param>
    /// <param name="limit">The boundary value that triggers the limit alert.</param>
    /// <param name="alertTitle">Title for the limit-reached alert.</param>
    /// <param name="alertMessage">Message for the limit-reached alert.</param>
    private async Task AdjustFontSizeAsync(
        double step, double limit, string alertTitle, string alertMessage)
    {
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

        // Screen Reader Announcement: announce current font size to accessibility users
        int percentage = (int)(FontSizeMultiplier * 100);
        SemanticScreenReader.Default.Announce($"Font size changed to {percentage} percent.");

        HardwareHelper.PerformHaptic(HapticFeedbackType.Click);
    }

    /// <summary>
    /// Resets all settings to default values after user confirmation.
    /// Demonstrates validation (confirmation dialog before destructive action).
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
            // Suppress callbacks to prevent cascading partial-method re-entrancy
            _isSyncingTheme = true;

            IsHighContrast = false;
            IsDarkMode = false;
            FontSizeMultiplier = 1.0;
            SelectedTheme = "System";

            _isSyncingTheme = false;

            App.SetTheme("System");
            Preferences.Set("FontSizeMultiplier", 1.0);
            Preferences.Set("HighContrast", false);
            Preferences.Set("AppTheme", "System");
            UpdateFontSizeLabel();

            // Screen Reader Announcement: confirm reset to accessibility users
            SemanticScreenReader.Default.Announce(
                "Settings have been reset to defaults. Theme is System, font size is Normal.");

            HardwareHelper.Vibrate(200);
        }
        catch (Exception ex)
        {
            _isSyncingTheme = false;
            await Shell.Current.DisplayAlert("Error",
                $"Unable to reset settings: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[SettingsVM] Reset error: {ex}");
        }
    }

    /// <summary>
    /// Updates the font size display label based on current multiplier value.
    /// Uses a switch expression for concise, readable mapping (KISS principle).
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
namespace FoodLens;

/// <summary>
/// Main application class. Manages theme switching, dynamic font scaling (WCAG 1.4.4),
/// and high contrast mode (WCAG 1.4.6). Colour resources are updated based on the
/// current combination of theme and contrast settings so text is always readable.
/// </summary>
public partial class App : Application
{
    /// <summary>Base font sizes before any multiplier is applied.</summary>
    private static readonly Dictionary<string, double> BaseFontSizes = new()
    {
        { "FontSizeCaption", 12.0 },
        { "FontSizeBody", 14.0 },
        { "FontSizeSubtitle", 16.0 },
        { "FontSizeHeader", 18.0 },
        { "FontSizeTitle", 24.0 },
        { "FontSizePageTitle", 26.0 }
    };

    /// <summary>Tracks whether high contrast is currently active.</summary>
    private static bool _isHighContrast;

    /// <summary>Tracks the current theme name.</summary>
    private static string _currentTheme = "System";

    /// <summary>
    /// Initialises the application, loads saved preferences, and applies them.
    /// </summary>
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();

        // Load saved preferences
        _currentTheme = Preferences.Get("AppTheme", "System");
        _isHighContrast = Preferences.Get("HighContrast", false);
        double savedMultiplier = Preferences.Get("FontSizeMultiplier", 1.0);

        // Apply theme first, then colours (which depend on theme), then font size
        SetTheme(_currentTheme);
        ApplyColours();
        ApplyFontSize(savedMultiplier);
    }

    /// <summary>
    /// Sets the application theme and refreshes all semantic colours.
    /// </summary>
    /// <param name="theme">Theme name: "Light", "Dark", or "System".</param>
    public static void SetTheme(string theme)
    {
        if (Application.Current is null)
        {
            return;
        }

        _currentTheme = theme;

        Application.Current.UserAppTheme = theme switch
        {
            "Dark" => AppTheme.Dark,
            "Light" => AppTheme.Light,
            _ => AppTheme.Unspecified
        };

        // Refresh colours because they depend on the active theme
        ApplyColours();
    }

    /// <summary>
    /// Enables or disables high contrast mode and refreshes all semantic colours.
    /// </summary>
    /// <param name="enabled">Whether high contrast should be active.</param>
    public static void ApplyHighContrast(bool enabled)
    {
        _isHighContrast = enabled;
        ApplyColours();
    }

    /// <summary>
    /// Updates all semantic colour resources based on the current combination of
    /// theme (Light/Dark/System) and high contrast (on/off).
    ///
    /// This ensures text is ALWAYS readable regardless of which combination the user selects:
    /// - Light + Normal: dark text on white background
    /// - Light + High Contrast: black background, white/yellow text (WCAG AAA)
    /// - Dark + Normal: white text on dark background
    /// - Dark + High Contrast: black background, white/yellow text (WCAG AAA)
    ///
    /// All XAML pages reference these semantic keys via {DynamicResource}, so changes
    /// take effect immediately without restarting the app.
    /// </summary>
    public static void ApplyColours()
    {
        if (Application.Current?.Resources is null)
        {
            return;
        }

        var resources = Application.Current.Resources;

        // Determine if we are effectively in dark mode
        bool isDark = _currentTheme == "Dark" ||
                      (_currentTheme == "System" &&
                       Application.Current.RequestedTheme == AppTheme.Dark);

        if (_isHighContrast)
        {
            // High Contrast mode — WCAG AAA 7:1 ratio
            // Same colours for both light and dark base themes to ensure maximum contrast
            resources["PageBackgroundColor"] = Color.FromArgb("#000000");
            resources["CardBackgroundColor"] = Color.FromArgb("#1A1A1A");
            resources["TextPrimaryColor"] = Color.FromArgb("#FFFFFF");
            resources["TextSecondaryColor"] = Color.FromArgb("#FFFF00");
            resources["BorderColor"] = Color.FromArgb("#FFFFFF");
            resources["PrimaryColor"] = Color.FromArgb("#00E676");
            resources["SecondaryColor"] = Color.FromArgb("#FFD600");
            resources["ErrorColor"] = Color.FromArgb("#FF5252");
            resources["InfoBackgroundColor"] = Color.FromArgb("#1A1A1A");
            resources["InfoTextColor"] = Color.FromArgb("#FFFFFF");
            resources["ButtonTextColor"] = Color.FromArgb("#000000");
            resources["MutedButtonBackgroundColor"] = Color.FromArgb("#FFFFFF");
            resources["MutedButtonTextColor"] = Color.FromArgb("#000000");
        }
        else if (isDark)
        {
            // Dark mode — WCAG AA 4.5:1 ratio
            resources["PageBackgroundColor"] = Color.FromArgb("#121212");
            resources["CardBackgroundColor"] = Color.FromArgb("#1E1E1E");
            resources["TextPrimaryColor"] = Color.FromArgb("#FFFFFF");
            resources["TextSecondaryColor"] = Color.FromArgb("#B0B0B0");
            resources["BorderColor"] = Color.FromArgb("#333333");
            resources["PrimaryColor"] = Color.FromArgb("#4CAF50");
            resources["SecondaryColor"] = Color.FromArgb("#FFB74D");
            resources["ErrorColor"] = Color.FromArgb("#EF5350");
            resources["InfoBackgroundColor"] = Color.FromArgb("#1B5E20");
            resources["InfoTextColor"] = Color.FromArgb("#E0E0E0");
            resources["ButtonTextColor"] = Color.FromArgb("#FFFFFF");
            resources["MutedButtonBackgroundColor"] = Color.FromArgb("#424242");
            resources["MutedButtonTextColor"] = Color.FromArgb("#FFFFFF");
        }
        else
        {
            // Light mode — WCAG AA 4.5:1 ratio (default)
            resources["PageBackgroundColor"] = Color.FromArgb("#FFFFFF");
            resources["CardBackgroundColor"] = Color.FromArgb("#FFFFFF");
            resources["TextPrimaryColor"] = Color.FromArgb("#212121");
            resources["TextSecondaryColor"] = Color.FromArgb("#616161");
            resources["BorderColor"] = Color.FromArgb("#E0E0E0");
            resources["PrimaryColor"] = Color.FromArgb("#2E7D32");
            resources["SecondaryColor"] = Color.FromArgb("#FF6F00");
            resources["ErrorColor"] = Color.FromArgb("#D32F2F");
            resources["InfoBackgroundColor"] = Color.FromArgb("#E8F5E9");
            resources["InfoTextColor"] = Color.FromArgb("#424242");
            resources["ButtonTextColor"] = Color.FromArgb("#FFFFFF");
            resources["MutedButtonBackgroundColor"] = Color.FromArgb("#E0E0E0");
            resources["MutedButtonTextColor"] = Color.FromArgb("#212121");
        }
    }

    /// <summary>
    /// Applies a font size multiplier to all dynamic font size resources.
    /// Enables WCAG 1.4.4 Resize Text — users can scale text from 75% to 200%.
    /// </summary>
    /// <param name="multiplier">The font size multiplier (0.75 to 2.0).</param>
    public static void ApplyFontSize(double multiplier)
    {
        if (Application.Current?.Resources is null)
        {
            return;
        }

        var resources = Application.Current.Resources;

        foreach (var kvp in BaseFontSizes)
        {
            double scaledSize = Math.Round(kvp.Value * multiplier, 1);
            resources[kvp.Key] = scaledSize;
        }
    }
}
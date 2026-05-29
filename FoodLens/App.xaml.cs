namespace FoodLens;

/// <summary>
/// Main application class. Handles app-level configuration including theme switching,
/// dynamic font scaling (WCAG 1.4.4), and high contrast mode (WCAG 1.4.3).
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

    /// <summary>
    /// Initialises the application, loads saved preferences, and applies them.
    /// </summary>
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();

        // Load and apply saved theme preference on startup
        string savedTheme = Preferences.Get("AppTheme", "System");
        SetTheme(savedTheme);

        // Load and apply saved font size multiplier (WCAG 1.4.4 Resize Text)
        double savedMultiplier = Preferences.Get("FontSizeMultiplier", 1.0);
        ApplyFontSize(savedMultiplier);

        // Load and apply saved high contrast preference (WCAG 1.4.3 Contrast)
        bool savedHighContrast = Preferences.Get("HighContrast", false);
        ApplyHighContrast(savedHighContrast);
    }

    /// <summary>
    /// Sets the application theme (Light, Dark, or System default).
    /// Addresses WCAG 1.4.3 Contrast and user preference requirements.
    /// </summary>
    /// <param name="theme">Theme name: "Light", "Dark", or "System".</param>
    public static void SetTheme(string theme)
    {
        if (Application.Current is null)
        {
            return;
        }

        Application.Current.UserAppTheme = theme switch
        {
            "Dark" => AppTheme.Dark,
            "Light" => AppTheme.Light,
            _ => AppTheme.Unspecified
        };
    }

    /// <summary>
    /// Applies a font size multiplier to all dynamic font size resources.
    /// This enables WCAG 1.4.4 Resize Text compliance — users can scale
    /// text from 75% to 200% of the base size without loss of content.
    /// Resources are updated via the app-level ResourceDictionary so that
    /// any XAML element using {DynamicResource FontSizeXxx} updates immediately.
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

    /// <summary>
    /// Applies or removes high contrast mode by updating colour resources.
    /// High contrast mode uses WCAG AAA 7:1 contrast ratio colours
    /// (pure white text on pure black background) for users with low vision.
    /// This directly addresses WCAG 1.4.3 Contrast (Minimum) and 1.4.6 Contrast (Enhanced).
    /// </summary>
    /// <param name="enabled">Whether high contrast mode should be active.</param>
    public static void ApplyHighContrast(bool enabled)
    {
        if (Application.Current?.Resources is null)
        {
            return;
        }

        var resources = Application.Current.Resources;

        if (enabled)
        {
            // Apply high contrast colours (WCAG AAA 7:1 ratio)
            resources["BackgroundLight"] = Color.FromArgb("#000000");
            resources["TextPrimaryLight"] = Color.FromArgb("#FFFFFF");
            resources["TextSecondaryLight"] = Color.FromArgb("#FFFF00");
            resources["BackgroundDark"] = Color.FromArgb("#000000");
            resources["SurfaceDark"] = Color.FromArgb("#1A1A1A");
            resources["TextPrimaryDark"] = Color.FromArgb("#FFFFFF");
            resources["TextSecondaryDark"] = Color.FromArgb("#FFFF00");
            resources["PrimaryColor"] = Color.FromArgb("#00E676");
            resources["SecondaryColor"] = Color.FromArgb("#FFD600");
            resources["ErrorColor"] = Color.FromArgb("#FF5252");
        }
        else
        {
            // Restore default colours (WCAG AA 4.5:1 ratio)
            resources["BackgroundLight"] = Color.FromArgb("#FFFFFF");
            resources["TextPrimaryLight"] = Color.FromArgb("#212121");
            resources["TextSecondaryLight"] = Color.FromArgb("#616161");
            resources["BackgroundDark"] = Color.FromArgb("#121212");
            resources["SurfaceDark"] = Color.FromArgb("#1E1E1E");
            resources["TextPrimaryDark"] = Color.FromArgb("#FFFFFF");
            resources["TextSecondaryDark"] = Color.FromArgb("#B0B0B0");
            resources["PrimaryColor"] = Color.FromArgb("#2E7D32");
            resources["SecondaryColor"] = Color.FromArgb("#FF6F00");
            resources["ErrorColor"] = Color.FromArgb("#D32F2F");
        }
    }
}
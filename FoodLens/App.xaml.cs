namespace FoodLens;

/// <summary>
/// Main application class. Handles app-wide configuration including theme management.
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Load saved theme preference on startup
        string savedTheme = Preferences.Get("AppTheme", "System");
        SetTheme(savedTheme);

        MainPage = new AppShell();
    }

    /// <summary>
    /// Sets the application theme (Light, Dark, or System default).
    /// This supports WCAG accessibility by allowing users to choose their preferred theme.
    /// </summary>
    /// <param name="theme">Theme name: "Light", "Dark", or "System"</param>
    public static void SetTheme(string theme)
    {
        if (Application.Current is null) return;

        Application.Current.UserAppTheme = theme switch
        {
            "Dark" => AppTheme.Dark,
            "Light" => AppTheme.Light,
            _ => AppTheme.Unspecified
        };
    }
}
using FoodLens.Views;

namespace FoodLens;

/// <summary>
/// Shell configuration for app navigation.
/// Registers routes for pages that are navigated to programmatically.
/// </summary>
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register detail page route for programmatic navigation
        Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
    }
}
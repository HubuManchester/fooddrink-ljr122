using FoodLens.Views;

namespace FoodLens;

/// <summary>
/// Shell configuration and route registration for navigation.
/// </summary>
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register detail page route for navigation
        Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
    }
}
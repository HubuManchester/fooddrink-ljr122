using FoodLens.ViewModels;

namespace FoodLens.Views;

/// <summary>
/// Code-behind for the Recipe Detail page.
/// Displays full recipe information with TTS and map features.
/// </summary>
public partial class RecipeDetailPage : ContentPage
{
    public RecipeDetailPage(RecipeDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodLens.Models;
using FoodLens.Services;
using FoodLens.Views;
using IntelliJ.Lang.Annotations;
using System.Collections.ObjectModel;
using static Android.Icu.Text.CaseMap;

namespace FoodLens.ViewModels;

/// <summary>
/// ViewModel for the main recipes list page.
/// Handles loading, filtering, searching, and shake-to-discover.
/// </summary>
public partial class RecipesViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;

    /// <summary>Observable collection of recipes for the list view.</summary>
    public ObservableCollection<Recipe> Recipes { get; } = new();

    /// <summary>Available filter categories.</summary>
    public ObservableCollection<string> Categories { get; } = new()
    {
        "All", "Breakfast", "Lunch", "Dinner", "Dessert", "Drinks"
    };

    /// <summary>Currently selected category filter.</summary>
    [ObservableProperty]
    private string _selectedCategory = "All";

    /// <summary>Search text entered by user.</summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    public RecipesViewModel(RecipeService recipeService)
    {
        Title = "Recipes";
        _recipeService = recipeService;
    }

    /// <summary>
    /// Loads recipes from the service with current filter applied.
    /// Includes comprehensive error handling.
    /// </summary>
    [RelayCommand]
    private async Task GetRecipesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            List<Recipe> recipes;

            // Apply search if text is present, otherwise filter by category
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                recipes = await _recipeService.SearchRecipesAsync(SearchText);
            }
            else
            {
                recipes = await _recipeService.GetRecipesByCategoryAsync(SelectedCategory);
            }

            if (Recipes.Count != 0)
                Recipes.Clear();

            foreach (var recipe in recipes)
            {
                Recipes.Add(recipe);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Error Loading Recipes",
                $"Something went wrong while loading recipes. Please try again.\n\nDetails: {ex.Message}",
                "OK");
            System.Diagnostics.Debug.WriteLine($"[RecipesViewModel] Error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Navigates to recipe detail page with haptic feedback.
    /// </summary>
    [RelayCommand]
    private async Task GoToDetailAsync(Recipe recipe)
    {
        if (recipe is null) return;

        try
        {
            // Haptic feedback on tap (hardware feature)
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (FeatureNotSupportedException)
        {
            System.Diagnostics.Debug.WriteLine("[RecipesViewModel] Haptic feedback not supported on this device.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipesViewModel] Haptic error: {ex.Message}");
        }

        await Shell.Current.GoToAsync(nameof(RecipeDetailPage), true, new Dictionary<string, object>
        {
            { "Recipe", recipe }
        });
    }

    /// <summary>
    /// Shake-to-discover: selects a random recipe with vibration feedback.
    /// Uses accelerometer shake detection and vibration hardware.
    /// </summary>
    [RelayCommand]
    private async Task ShakeDiscoverAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // Vibration feedback to confirm shake detected (hardware feature)
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(400));

            var randomRecipe = await _recipeService.GetRandomRecipeAsync();

            await Shell.Current.GoToAsync(nameof(RecipeDetailPage), true, new Dictionary<string, object>
            {
                { "Recipe", randomRecipe }
            });
        }
        catch (FeatureNotSupportedException)
        {
            await Shell.Current.DisplayAlert("Not Supported",
                "Vibration is not supported on this device.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error",
                $"Unable to get random recipe: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Reloads recipes when category filter changes.
    /// </summary>
    partial void OnSelectedCategoryChanged(string value)
    {
        MainThread.BeginInvokeOnMainThread(async () => await GetRecipesAsync());
    }

    /// <summary>
    /// Triggers search when search text changes.
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        MainThread.BeginInvokeOnMainThread(async () => await GetRecipesAsync());
    }
}
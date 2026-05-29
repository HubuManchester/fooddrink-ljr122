using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodLens.Helpers;
using FoodLens.Models;
using FoodLens.Services;
using FoodLens.Views;
using System.Collections.ObjectModel;

namespace FoodLens.ViewModels;

/// <summary>
/// ViewModel for the main recipes list page.
/// Handles loading, filtering, searching, shake-to-discover,
/// swipe-to-favourite gesture, and share functionality.
/// </summary>
public partial class RecipesViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;

    /// <summary>
    /// Debounce cancellation token source for search text changes.
    /// Protected by a lock to ensure thread-safe replacement and disposal.
    /// </summary>
    private CancellationTokenSource? _searchDebounceCts;

    /// <summary>
    /// Lock object for thread-safe access to <see cref="_searchDebounceCts"/>.
    /// </summary>
    private readonly object _debounceLock = new();

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

    /// <summary>Toast message shown after swipe-to-favourite action.</summary>
    [ObservableProperty]
    private string _toastMessage = string.Empty;

    /// <summary>Controls visibility of the toast notification.</summary>
    [ObservableProperty]
    private bool _isToastVisible;

    /// <summary>
    /// Initialises the RecipesViewModel with the recipe service dependency.
    /// </summary>
    /// <param name="recipeService">Injected recipe data service.</param>
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
        if (IsBusy)
        {
            return;
        }

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
            {
                Recipes.Clear();
            }

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
            System.Diagnostics.Debug.WriteLine($"[RecipesVM] Error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Sets the selected category and reloads recipes.
    /// Called by category filter buttons in the UI via CommandParameter.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    [RelayCommand]
    private async Task FilterByCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return;
        }

        try
        {
            // Haptic feedback on category selection (HARDWARE: Haptic Feedback)
            HardwareHelper.PerformHaptic(HapticFeedbackType.Click);

            SelectedCategory = category;
            SearchText = string.Empty; // Clear search when filtering by category
            await GetRecipesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipesVM] Filter error: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigates to recipe detail page with haptic feedback.
    /// </summary>
    /// <param name="recipe">The recipe to view in detail.</param>
    [RelayCommand]
    private async Task GoToDetailAsync(Recipe recipe)
    {
        if (recipe is null)
        {
            return;
        }

        // Haptic feedback on tap (HARDWARE: Haptic Feedback)
        HardwareHelper.PerformHaptic(HapticFeedbackType.Click);

        await Shell.Current.GoToAsync(nameof(RecipeDetailPage), true, new Dictionary<string, object>
        {
            { "Recipe", recipe }
        });
    }

    /// <summary>
    /// Swipe-to-favourite: toggles favourite status when user swipes right on a recipe card.
    /// Demonstrates advanced gesture-based functionality (swipe interaction).
    /// </summary>
    /// <param name="recipe">The recipe to toggle favourite status for.</param>
    [RelayCommand]
    private async Task SwipeFavouriteAsync(Recipe recipe)
    {
        if (recipe is null)
        {
            return;
        }

        try
        {
            bool isFav = await _recipeService.ToggleFavouriteAsync(recipe.Id);

            // Haptic feedback for swipe action (HARDWARE: Haptic Feedback)
            HardwareHelper.PerformHaptic(HapticFeedbackType.LongPress);

            // Show toast notification
            ToastMessage = isFav
                ? $"❤️ {recipe.Name} added to favourites!"
                : $"💔 {recipe.Name} removed from favourites.";
            IsToastVisible = true;

            // Refresh the list to update the favourite indicator
            await GetRecipesAsync();

            // Auto-hide toast after 2.5 seconds
            await Task.Delay(2500);
            IsToastVisible = false;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error",
                $"Unable to update favourite: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[RecipesVM] Favourite error: {ex}");
        }
    }

    /// <summary>
    /// Shares a recipe via the device's native share sheet.
    /// Triggered by swiping left on a recipe card (advanced gesture interaction).
    /// </summary>
    /// <param name="recipe">The recipe to share.</param>
    [RelayCommand]
    private async Task ShareRecipeAsync(Recipe recipe)
    {
        if (recipe is null)
        {
            return;
        }

        try
        {
            // Haptic feedback on share action (HARDWARE: Haptic Feedback)
            HardwareHelper.PerformHaptic(HapticFeedbackType.Click);

            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = $"Share: {recipe.Name}",
                Text = $"🍽️ Check out this recipe: {recipe.Name}\n\n" +
                       $"{recipe.Description}\n\n" +
                       $"⏱️ Total time: {recipe.TotalTimeMinutes} minutes\n" +
                       $"📊 Difficulty: {recipe.Difficulty}\n" +
                       $"📍 Origin: {recipe.Origin}\n\n" +
                       $"Shared from FoodLens"
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Share Error",
                $"Unable to share recipe: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[RecipesVM] Share error: {ex}");
        }
    }

    /// <summary>
    /// Shake-to-discover: selects a random recipe with vibration feedback.
    /// Uses accelerometer shake detection and vibration hardware.
    /// HARDWARE FEATURES: Vibration, Accelerometer (shake detected in code-behind).
    /// </summary>
    [RelayCommand]
    private async Task ShakeDiscoverAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;

            // Vibration feedback to confirm shake detected (HARDWARE: Vibration)
            HardwareHelper.Vibrate(400);

            var randomRecipe = await _recipeService.GetRandomRecipeAsync();

            await Shell.Current.GoToAsync(nameof(RecipeDetailPage), true, new Dictionary<string, object>
            {
                { "Recipe", randomRecipe }
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error",
                $"Unable to get random recipe: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[RecipesVM] Shake discover error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Triggers search with debounce when search text changes.
    /// Uses a 350ms debounce to avoid firing a search on every keystroke,
    /// which improves performance and reduces unnecessary processing.
    ///
    /// FIX: The previous version created a new <see cref="CancellationTokenSource"/> without
    /// disposing the old one, causing a CA2000 resource-leak warning from Roslyn.
    /// The replacement now acquires <see cref="_debounceLock"/>, cancels AND disposes the
    /// previous instance, then assigns the new one — eliminating the leak.
    /// </summary>
    /// <param name="value">The new search text value.</param>
    partial void OnSearchTextChanged(string value)
    {
        CancellationTokenSource newCts;

        // FIX: Dispose the old CTS before replacing it.
        // Roslyn CA2000 warns when an IDisposable is created but never explicitly disposed.
        // CancellationTokenSource implements IDisposable and holds a WaitHandle that is
        // only released on Dispose(). Without Dispose(), every keystroke leaked a handle.
        lock (_debounceLock)
        {
            _searchDebounceCts?.Cancel();
            _searchDebounceCts?.Dispose();       // <-- the fix
            _searchDebounceCts = new CancellationTokenSource();
            newCts = _searchDebounceCts;
        }

        // Fire search after 350ms debounce delay
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(350, newCts.Token);

                if (!newCts.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await GetRecipesAsync();
                    });
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when user types another character before delay completes
            }
        }, newCts.Token);
    }
}
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
///
/// Key design patterns used:
/// - Repository pattern: delegates all data access to <see cref="RecipeService"/>.
/// - Debounce pattern: search text changes are debounced (350ms) to avoid
///   firing a new query on every keystroke, improving performance (KISS).
/// - DRY: all haptic feedback goes through <see cref="HardwareHelper"/>
///   rather than repeating try-catch blocks in every method.
///
/// FIX (Roslyn CA2213): Implements <see cref="IDisposable"/> to correctly release
/// the <see cref="_searchDebounceCts"/> CancellationTokenSource when the ViewModel
/// is no longer needed. Without disposal, the WaitHandle inside CTS leaks indefinitely.
/// The DI container (AddSingleton) holds a reference for the app's lifetime, but
/// implementing IDisposable is still correct practice and satisfies the analyser.
/// </summary>
public partial class RecipesViewModel : BaseViewModel, IDisposable
{
    private readonly RecipeService _recipeService;

    /// <summary>
    /// Debounce cancellation token source for search text changes.
    /// Protected by <see cref="_debounceLock"/> to ensure thread-safe replacement.
    /// </summary>
    private CancellationTokenSource? _searchDebounceCts;

    /// <summary>
    /// Lock object for thread-safe access to <see cref="_searchDebounceCts"/>.
    /// Prevents a race condition where two rapid keystrokes could dispose the
    /// same CancellationTokenSource from different threads simultaneously.
    /// </summary>
    private readonly object _debounceLock = new();

    /// <summary>Tracks whether Dispose has already been called (CA2213).</summary>
    private bool _disposed;

    /// <summary>Observable collection of recipes bound to the list view.</summary>
    public ObservableCollection<Recipe> Recipes { get; } = [];

    /// <summary>Available filter category options for the toolbar.</summary>
    public ObservableCollection<string> Categories { get; } =
    [
        "All", "Breakfast", "Lunch", "Dinner", "Dessert", "Drinks"
    ];

    /// <summary>Currently selected category filter.</summary>
    [ObservableProperty]
    private string _selectedCategory = "All";

    /// <summary>Search text entered by the user. Changes trigger a debounced reload.</summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>Toast message shown after swipe-to-favourite or swipe-to-share action.</summary>
    [ObservableProperty]
    private string _toastMessage = string.Empty;

    /// <summary>Controls visibility of the toast notification overlay.</summary>
    [ObservableProperty]
    private bool _isToastVisible;

    /// <summary>
    /// Initialises the RecipesViewModel with the injected recipe service.
    /// </summary>
    /// <param name="recipeService">Data service providing recipe data.</param>
    public RecipesViewModel(RecipeService recipeService)
    {
        Title = "Recipes";
        _recipeService = recipeService;
    }

    /// <summary>
    /// Loads recipes from the service with the current filter or search applied.
    /// Includes comprehensive error handling — all exceptions are caught and
    /// displayed to the user via DisplayAlert so the app never crashes silently.
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

            // Clear the existing collection before adding new items
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
    /// Sets the selected category and reloads the recipe list.
    /// Called by category filter buttons via CommandParameter binding.
    /// Clears the search text so category and search filters don't conflict.
    /// </summary>
    /// <param name="category">The category name to filter by.</param>
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
    /// Navigates to the recipe detail page with haptic feedback.
    /// </summary>
    /// <param name="recipe">The recipe to display in detail.</param>
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
    /// Swipe-to-favourite: toggles the favourite status when the user swipes right.
    /// Demonstrates advanced gesture-based interaction (SwipeView) with haptic feedback.
    /// </summary>
    /// <param name="recipe">The recipe whose favourite status should be toggled.</param>
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

            // Show a toast to confirm the action to the user
            ToastMessage = isFav
                ? $"❤️ {recipe.Name} added to favourites!"
                : $"💔 {recipe.Name} removed from favourites.";
            IsToastVisible = true;

            // Refresh the list to update the favourite indicator on the card
            await GetRecipesAsync();

            // Auto-hide the toast after 2.5 seconds
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
    /// Shake-to-discover: selects a random recipe and navigates to its detail page.
    /// Triggered either by the floating action button or by the physical shake gesture
    /// detected via the accelerometer in <see cref="RecipesPage"/> code-behind.
    ///
    /// HARDWARE FEATURES:
    /// - Vibration: 400ms pulse confirms the shake was detected.
    /// - Accelerometer: shake is detected in the page's code-behind via
    ///   Accelerometer.Default.ShakeDetected, then delegated to this command.
    /// </summary>
    [RelayCommand]
    private async Task ShakeDiscoverAsync()
    {
        if (IsBusy)
        {
            // Provide feedback even when the app is busy so the user knows
            // their action was registered but must wait for the current operation.
            HardwareHelper.PerformHaptic(HapticFeedbackType.LongPress);
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
    /// Triggers a search with debounce when the search text property changes.
    /// Uses a 350ms debounce delay to avoid firing a new database/service query
    /// on every single keystroke, which would be wasteful and cause UI jank.
    ///
    /// FIX (Roslyn CA2000): The previous implementation created a new
    /// <see cref="CancellationTokenSource"/> without disposing the previous one,
    /// leaking a WaitHandle on every keystroke. The fix acquires
    /// <see cref="_debounceLock"/>, cancels AND disposes the old instance,
    /// then assigns the new one — eliminating the resource leak entirely.
    /// </summary>
    /// <param name="value">The new search text value.</param>
    partial void OnSearchTextChanged(string value)
    {
        CancellationTokenSource newCts;

        // Acquire lock to safely cancel + dispose old CTS and create new one
        lock (_debounceLock)
        {
            _searchDebounceCts?.Cancel();
            _searchDebounceCts?.Dispose(); // Fix CA2000: dispose old CTS to release WaitHandle
            _searchDebounceCts = new CancellationTokenSource();
            newCts = _searchDebounceCts;
        }

        // Fire the search after the 350ms debounce window
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
                // Expected when the user types another character before the delay completes
            }
        }, newCts.Token);
    }

    /// <summary>
    /// Releases managed resources.
    /// FIX (Roslyn CA2213): Disposes the <see cref="_searchDebounceCts"/> field
    /// so the underlying WaitHandle is released when the ViewModel is no longer
    /// needed. This satisfies the analyser and is correct dispose hygiene.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose method following the standard Dispose pattern.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(); false if from finaliser.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            lock (_debounceLock)
            {
                _searchDebounceCts?.Cancel();
                _searchDebounceCts?.Dispose();
                _searchDebounceCts = null;
            }
        }

        _disposed = true;
    }
}
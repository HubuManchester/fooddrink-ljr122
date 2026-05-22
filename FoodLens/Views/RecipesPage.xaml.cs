using FoodLens.ViewModels;

namespace FoodLens.Views;

/// <summary>
/// Code-behind for the Recipes list page.
/// Handles accelerometer shake detection for the "shake to discover" feature.
/// Hardware feature: Accelerometer (Shake detection).
/// </summary>
public partial class RecipesPage : ContentPage
{
    private readonly RecipesViewModel _viewModel;

    public RecipesPage(RecipesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    /// <summary>
    /// Called when the page appears. Loads recipes and starts shake detection.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Load recipes on first appearance
        if (_viewModel.Recipes.Count == 0)
        {
            await _viewModel.GetRecipesCommand.ExecuteAsync(null);
        }

        // Start accelerometer shake detection (HARDWARE FEATURE: Accelerometer/Shake)
        StartShakeDetection();
    }

    /// <summary>
    /// Called when the page disappears. Stops shake detection to save battery.
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopShakeDetection();
    }

    /// <summary>
    /// Starts monitoring the accelerometer for shake gestures.
    /// Uses the device's accelerometer hardware sensor.
    /// </summary>
    private void StartShakeDetection()
    {
        try
        {
            if (Accelerometer.Default.IsSupported)
            {
                if (!Accelerometer.Default.IsMonitoring)
                {
                    Accelerometer.Default.ShakeDetected += OnShakeDetected;
                    Accelerometer.Default.Start(SensorSpeed.Game);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[RecipesPage] Accelerometer not supported on this device.");
            }
        }
        catch (FeatureNotSupportedException)
        {
            System.Diagnostics.Debug.WriteLine("[RecipesPage] Accelerometer feature not supported.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipesPage] Accelerometer start error: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops accelerometer monitoring to conserve battery life.
    /// </summary>
    private void StopShakeDetection()
    {
        try
        {
            if (Accelerometer.Default.IsMonitoring)
            {
                Accelerometer.Default.ShakeDetected -= OnShakeDetected;
                Accelerometer.Default.Stop();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipesPage] Accelerometer stop error: {ex.Message}");
        }
    }

    /// <summary>
    /// Event handler triggered when a shake gesture is detected.
    /// Navigates to a random recipe with vibration feedback.
    /// </summary>
    private async void OnShakeDetected(object? sender, EventArgs e)
    {
        // Execute shake discover command on the main thread
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await _viewModel.ShakeDiscoverCommand.ExecuteAsync(null);
        });
    }
}
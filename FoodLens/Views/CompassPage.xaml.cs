using FoodLens.ViewModels;

namespace FoodLens.Views;

/// <summary>
/// Code-behind for the Compass page.
/// HARDWARE FEATURE: Compass/Magnetometer.
/// Demonstrates real-time sensor reading from the device's magnetometer.
/// On Android emulators, use Extended Controls > Virtual Sensors to manipulate
/// the magnetic field and see the compass heading change live on screen.
/// </summary>
public partial class CompassPage : ContentPage
{
    private readonly CompassViewModel _viewModel;

    /// <summary>
    /// Initialises the CompassPage with the injected ViewModel.
    /// </summary>
    /// <param name="viewModel">The compass ViewModel provided by DI.</param>
    public CompassPage(CompassViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    /// <summary>
    /// Cleans up the compass sensor when navigating away from this page.
    /// Stops the magnetometer to conserve battery and prevent resource leaks.
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.Cleanup();
    }
}
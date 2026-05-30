using FoodLens.ViewModels;

namespace FoodLens.Views;

/// <summary>
/// Code-behind for the Camera/Food Scanner page.
/// Demonstrates camera hardware usage and networking via the Open Food Facts API.
/// HARDWARE FEATURES: Camera, Haptic Feedback, Vibration.
/// NETWORKING: Open Food Facts REST API for real nutritional data.
/// </summary>
public partial class CameraPage : ContentPage
{
    /// <summary>
    /// Initialises the CameraPage with the injected ViewModel.
    /// </summary>
    /// <param name="viewModel">The camera ViewModel provided by DI.</param>
    public CameraPage(CameraViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
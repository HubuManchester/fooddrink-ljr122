using FoodLens.ViewModels;

namespace FoodLens.Views;

/// <summary>
/// Code-behind for the Camera/Food Scanner page.
/// Uses device camera hardware for food identification.
/// </summary>
public partial class CameraPage : ContentPage
{
    public CameraPage(CameraViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
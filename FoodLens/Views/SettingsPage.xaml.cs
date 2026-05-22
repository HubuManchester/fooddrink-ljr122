using FoodLens.ViewModels;

namespace FoodLens.Views;

/// <summary>
/// Code-behind for the Settings page.
/// Provides accessibility customisation options including dark mode and font scaling.
/// Addresses WCAG 2.1 guidelines for user preferences.
/// </summary>
public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
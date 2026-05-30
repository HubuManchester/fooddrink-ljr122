using CommunityToolkit.Mvvm.ComponentModel;

namespace FoodLens.ViewModels;

/// <summary>
/// Base view model providing shared properties and behaviour for all ViewModels.
/// Inherits from <see cref="ObservableObject"/> (CommunityToolkit.Mvvm) so that
/// derived classes can use [ObservableProperty] source generators.
///
/// Follows the KISS principle — only contains what every ViewModel needs:
/// IsBusy for loading state and Title for the navigation bar.
/// All other responsibilities belong in the concrete ViewModel subclass.
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    /// <summary>
    /// Indicates whether an asynchronous operation is currently in progress.
    /// Bound to ActivityIndicator.IsRunning and Button.IsEnabled (via IsNotBusy)
    /// to give the user visual feedback and prevent duplicate actions.
    /// </summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Page title displayed in the navigation bar.
    /// Each concrete ViewModel sets this in its constructor.
    /// </summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>
    /// Inverse of <see cref="IsBusy"/> used to enable/disable UI controls via binding.
    /// Exposed as a computed property rather than a separate observable field
    /// to keep the single source of truth in IsBusy (DRY principle).
    /// </summary>
    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// Called by the source-generated setter whenever <see cref="IsBusy"/> changes.
    /// Raises a property-changed notification for <see cref="IsNotBusy"/> so that
    /// any controls bound to IsNotBusy also update when IsBusy is toggled.
    /// Without this, IsNotBusy would never notify the UI of its value change.
    /// </summary>
    /// <param name="value">The new value of IsBusy.</param>
    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }
}
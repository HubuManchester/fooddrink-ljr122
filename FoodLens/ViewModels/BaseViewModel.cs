using CommunityToolkit.Mvvm.ComponentModel;

namespace FoodLens.ViewModels;

/// <summary>
/// Base view model providing common properties for all view models.
/// Uses CommunityToolkit.Mvvm source generators for observable properties.
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    /// <summary>Indicates whether data is currently loading.</summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>Page title for navigation bar.</summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>Inverse of IsBusy for binding enabled states.</summary>
    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// Notifies the UI that IsNotBusy has changed whenever IsBusy changes.
    /// Ensures bound controls update correctly.
    /// </summary>
    /// <param name="value">The new value of IsBusy.</param>
    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using IntelliJ.Lang.Annotations;

namespace FoodLens.ViewModels;

/// <summary>
/// Base view model providing common properties for all view models.
/// Uses CommunityToolkit.Mvvm source generators.
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
}
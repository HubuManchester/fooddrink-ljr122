using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodLens.Helpers;

namespace FoodLens.ViewModels;

/// <summary>
/// ViewModel for the Compass page.
/// HARDWARE FEATURE: Compass/Magnetometer.
/// Reads the device's magnetometer sensor to display real-time heading direction.
/// On Android emulators, this can be demonstrated via Extended Controls > Virtual Sensors,
/// where the magnetic field values can be manipulated to show live compass changes on screen.
/// </summary>
public partial class CompassViewModel : BaseViewModel
{
    /// <summary>Current compass heading in degrees (0-360).</summary>
    [ObservableProperty]
    private double _headingDegrees;

    /// <summary>Formatted heading text for display.</summary>
    [ObservableProperty]
    private string _headingText = "N 0°";

    /// <summary>Cardinal direction name (North, South, East, West, etc.).</summary>
    [ObservableProperty]
    private string _cardinalDirection = "North";

    /// <summary>Whether the compass sensor is currently active.</summary>
    [ObservableProperty]
    private bool _isMonitoring;

    /// <summary>Status message for the compass sensor.</summary>
    [ObservableProperty]
    private string _statusMessage = "Tap 'Start Compass' to begin reading the magnetometer sensor.";

    /// <summary>Rotation angle for the compass needle UI element (negative for correct visual rotation).</summary>
    [ObservableProperty]
    private double _needleRotation;

    /// <summary>
    /// Initialises the CompassViewModel with default title.
    /// </summary>
    public CompassViewModel()
    {
        Title = "Compass";
    }

    /// <summary>
    /// Starts or stops the compass/magnetometer sensor.
    /// HARDWARE FEATURE: Compass (Magnetometer).
    /// On Android emulator: use Extended Controls > Virtual Sensors > Magnetometer
    /// to rotate the virtual device and see the heading change in real time.
    /// </summary>
    [RelayCommand]
    private async Task ToggleCompassAsync()
    {
        if (IsMonitoring)
        {
            StopCompass();
        }
        else
        {
            await StartCompassAsync();
        }
    }

    /// <summary>
    /// Starts the compass sensor and subscribes to reading changes.
    /// </summary>
    private async Task StartCompassAsync()
    {
        try
        {
            if (!Compass.Default.IsSupported)
            {
                StatusMessage = "⚠️ Compass/Magnetometer is not supported on this device.";
                await Shell.Current.DisplayAlert("Not Supported",
                    "The compass (magnetometer) sensor is not available on this device or emulator. " +
                    "Please use an Android emulator with Virtual Sensors enabled.",
                    "OK");
                return;
            }

            if (Compass.Default.IsMonitoring)
            {
                return;
            }

            // Subscribe to compass reading changes
            Compass.Default.ReadingChanged += OnCompassReadingChanged;
            Compass.Default.Start(SensorSpeed.UI);

            IsMonitoring = true;
            StatusMessage = "🧭 Compass active — rotate the device to see heading changes.";

            // Haptic feedback to confirm sensor started (HARDWARE: Haptic Feedback)
            HardwareHelper.PerformHaptic(HapticFeedbackType.Click);
        }
        catch (FeatureNotSupportedException)
        {
            StatusMessage = "⚠️ Compass sensor is not supported on this device.";
            await Shell.Current.DisplayAlert("Not Supported",
                "Compass hardware is not available.", "OK");
        }
        catch (Exception ex)
        {
            StatusMessage = $"⚠️ Error starting compass: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[CompassVM] Start error: {ex}");
        }
    }

    /// <summary>
    /// Stops the compass sensor and unsubscribes from reading changes.
    /// </summary>
    private void StopCompass()
    {
        try
        {
            if (Compass.Default.IsMonitoring)
            {
                Compass.Default.ReadingChanged -= OnCompassReadingChanged;
                Compass.Default.Stop();
            }

            IsMonitoring = false;
            StatusMessage = "Compass stopped. Tap 'Start Compass' to resume.";

            HardwareHelper.PerformHaptic(HapticFeedbackType.Click);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CompassVM] Stop error: {ex}");
        }
    }

    /// <summary>
    /// Handles compass reading changes from the magnetometer sensor.
    /// Updates the heading, cardinal direction, and needle rotation on the UI thread.
    /// </summary>
    /// <param name="sender">The compass sensor.</param>
    /// <param name="e">Event args containing the new heading reading.</param>
    private void OnCompassReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        // Must update UI properties on the main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            HeadingDegrees = e.Reading.HeadingMagneticNorth;
            HeadingText = $"{GetCardinalAbbreviation(HeadingDegrees)} {HeadingDegrees:F0}°";
            CardinalDirection = GetCardinalDirectionName(HeadingDegrees);

            // Rotate the needle opposite to heading so it always points north
            NeedleRotation = -HeadingDegrees;
        });
    }

    /// <summary>
    /// Converts a heading in degrees to a cardinal direction abbreviation.
    /// </summary>
    /// <param name="heading">Heading in degrees (0-360).</param>
    /// <returns>Cardinal abbreviation (N, NE, E, SE, S, SW, W, NW).</returns>
    private static string GetCardinalAbbreviation(double heading)
    {
        return heading switch
        {
            >= 337.5 or < 22.5 => "N",
            >= 22.5 and < 67.5 => "NE",
            >= 67.5 and < 112.5 => "E",
            >= 112.5 and < 157.5 => "SE",
            >= 157.5 and < 202.5 => "S",
            >= 202.5 and < 247.5 => "SW",
            >= 247.5 and < 292.5 => "W",
            _ => "NW"
        };
    }

    /// <summary>
    /// Converts a heading in degrees to a full cardinal direction name.
    /// </summary>
    /// <param name="heading">Heading in degrees (0-360).</param>
    /// <returns>Full cardinal direction name.</returns>
    private static string GetCardinalDirectionName(double heading)
    {
        return heading switch
        {
            >= 337.5 or < 22.5 => "North",
            >= 22.5 and < 67.5 => "North-East",
            >= 67.5 and < 112.5 => "East",
            >= 112.5 and < 157.5 => "South-East",
            >= 157.5 and < 202.5 => "South",
            >= 202.5 and < 247.5 => "South-West",
            >= 247.5 and < 292.5 => "West",
            _ => "North-West"
        };
    }

    /// <summary>
    /// Cleans up the compass sensor when the ViewModel is no longer needed.
    /// Called by the page's OnDisappearing event.
    /// </summary>
    public void Cleanup()
    {
        StopCompass();
    }
}
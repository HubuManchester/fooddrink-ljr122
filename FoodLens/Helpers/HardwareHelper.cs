namespace FoodLens.Helpers;

/// <summary>
/// Reusable helper class for common hardware interactions.
/// Centralises haptic feedback and vibration calls to follow the DRY principle,
/// reducing repeated try-catch blocks throughout the codebase.
/// </summary>
public static class HardwareHelper
{
    /// <summary>
    /// Performs haptic feedback safely, catching FeatureNotSupportedException.
    /// </summary>
    /// <param name="type">The type of haptic feedback to perform.</param>
    public static void PerformHaptic(HapticFeedbackType type = HapticFeedbackType.Click)
    {
        try
        {
            HapticFeedback.Default.Perform(type);
        }
        catch (FeatureNotSupportedException)
        {
            System.Diagnostics.Debug.WriteLine("[HardwareHelper] Haptic feedback not supported.");
        }
    }

    /// <summary>
    /// Triggers device vibration safely for the specified duration.
    /// </summary>
    /// <param name="milliseconds">Duration of vibration in milliseconds.</param>
    public static void Vibrate(int milliseconds = 200)
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(milliseconds));
        }
        catch (FeatureNotSupportedException)
        {
            System.Diagnostics.Debug.WriteLine("[HardwareHelper] Vibration not supported.");
        }
    }
}
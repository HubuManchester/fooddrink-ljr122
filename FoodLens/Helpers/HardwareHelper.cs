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
    /// Wrapping in a try-catch here means every call-site is clean and concise,
    /// with no duplicated exception-handling boilerplate (KISS + DRY).
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
            // Haptic feedback is optional — silently log and continue.
            // Not all devices or emulators support haptic feedback.
            System.Diagnostics.Debug.WriteLine("[HardwareHelper] Haptic feedback not supported.");
        }
    }

    /// <summary>
    /// Triggers device vibration safely for the specified duration.
    /// Catches FeatureNotSupportedException so callers need no error handling.
    /// </summary>
    /// <param name="milliseconds">Duration of vibration in milliseconds. Default is 200ms.</param>
    public static void Vibrate(int milliseconds = 200)
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(milliseconds));
        }
        catch (FeatureNotSupportedException)
        {
            // Vibration is optional — silently log and continue.
            System.Diagnostics.Debug.WriteLine("[HardwareHelper] Vibration not supported.");
        }
    }
}
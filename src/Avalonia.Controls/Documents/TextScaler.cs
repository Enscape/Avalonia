using System;
using Avalonia.Platform;

namespace Avalonia.Controls.Documents;

public class TextScaler
{
    public bool IsTextScalingEnabled { get => field; set => SetTextScaleValue(ref field, value); } = true;
    public double MinScaledFontSize { get => field; set => SetTextScaleValue(ref field, value); }
    public double MaxScaledFontSize { get => field; set => SetTextScaleValue(ref field, value); } = double.PositiveInfinity;

    /// <summary>
    /// Raised when the system text scaling changes.
    /// </summary>
    public event EventHandler<EventArgs>? TextScalingChanged;

    /// <summary>
    /// Scales a font size for a specific element.
    /// </summary>
    /// <remarks>Text scaling is typically not uniform. Smaller text scales up faster than larger text.</remarks>
    public double GetScaledFontSize(IPlatformTextScaleable scaleable, double baseFontSize)
    {
        if (MaxScaledFontSize < MinScaledFontSize)
        {
            throw new InvalidOperationException($"{nameof(MaxScaledFontSize)} cannot be smaller than {nameof(MinScaledFontSize)}");
        }

        return !double.IsNaN(baseFontSize) && IsTextScalingEnabled ?
            Math.Clamp(GetScaledFontSizeOverride(scaleable, baseFontSize), MinScaledFontSize, MaxScaledFontSize) : baseFontSize;
    }

    /// <summary>
    /// Updates a field and calls <see cref="OnTextScalingChanged"/>, if the value has changed.
    /// </summary>
    protected void SetTextScaleValue<T>(ref T field, T value)
    {
        if (!Equals(field, value))
        {
            field = value;
            OnTextScalingChanged();
        }
    }

    /// <summary>
    /// Scales a font size according to <see cref="IPlatformSettings.GetScaledFontSize(double)"/>, unless overriden by a derived class.
    /// </summary>
    protected virtual double GetScaledFontSizeOverride(IPlatformTextScaleable scaleable, double baseFontSize) =>
        scaleable.PlatformSettings?.GetScaledFontSize(baseFontSize) ?? baseFontSize;

    /// <summary>
    /// Raises <see cref="TextScalingChanged"/>.
    /// </summary>
    /// <remarks>
    /// It is not necessary to call this method when platform text scaling changes.
    /// </remarks>
    protected virtual void OnTextScalingChanged() => TextScalingChanged?.Invoke(this, EventArgs.Empty);
}
